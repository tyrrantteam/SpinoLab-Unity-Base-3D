using System;
using DG.Tweening;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinGroup.UI.SwipeHub
{
    /// <summary>
    /// Horizontal pager: finger follows the strip 1:1, neighboring pages stay visible, then snap.
    /// Bottom tabs stay outside this viewport and do not move.
    /// </summary>
    [DisallowMultipleComponent]
    public class SwipePageHub : MonoBehaviour,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IInitializePotentialDragHandler
    {
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private SwipePage[] pages = Array.Empty<SwipePage>();
        [SerializeField] private SwipeTabItem[] tabs = Array.Empty<SwipeTabItem>();

        [Header("Page Size")]
        [SerializeField, Min(1f), Tooltip("Design page width. Relayout uses this as the default and scales from it when the screen size differs.")]
        private float pageWidth = 1080f;
        [SerializeField, Min(1f), Tooltip("Design page height. Relayout uses this as the default and scales from it when the screen size differs.")]
        private float pageHeight = 2160f;

        [SerializeField] private int initialPageIndex = 1;
        [SerializeField] private float snapDuration = 0.3f;
        [SerializeField] private DG.Tweening.Ease snapEase = DG.Tweening.Ease.OutCubic;
        [SerializeField, Range(0.05f, 0.5f)] private float changePageThreshold = 0.25f;
        [SerializeField] private float flickVelocity = 800f;
        [SerializeField, Range(0f, 1f)] private float rubberBand = 0.35f;

        [Header("Active Tab")]
        [SerializeField] private RectTransform activeIndicator;
        [SerializeField] private float activeMoveDuration = 0.25f;
        [SerializeField] private LitMotion.Ease activeMoveEase = LitMotion.Ease.OutCubic;

        public static SwipePageHub Instance { get; private set; }

        public event Action<int> OnPageChanged;
        public event Action<SwipePageKind> OnPageKindChanged;

        public int CurrentIndex => _currentIndex;

        public SwipePageKind CurrentPageKind =>
            HasPage(_currentIndex) ? pages[_currentIndex].PageKind : SwipePageKind.Home;

        private int _currentIndex = 1;
        private bool _dragging;
        private float _dragStartLocalX;
        private float _dragStartContentX;
        private float _lastLocalX;
        private float _lastTime;
        private float _horizontalVelocity;
        private Tweener _snapTween;
        private MotionHandle _activeMoveHandle;
        private Vector2[] _activeTargetsByPage;
        private bool _initialized;
        private Vector2 _screenSize;
        private float _scaledPageWidth;
        private float _scaledPageHeight;

        private void Awake()
        {
            Instance = this;
            _scaledPageWidth = PageWidth;
            _scaledPageHeight = PageHeight;
            EnsureViewportGraphic();
            BindTabs();
        }

        private void Start()
        {
            Relayout();
            SelectPage(Mathf.Clamp(initialPageIndex, 0, Mathf.Max(0, pages.Length - 1)), false);
            _initialized = true;
        }

        private void OnEnable()
        {
            if (!_initialized)
                return;

            Relayout();
            SetContentX(TargetX(_currentIndex), true);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || !_initialized || _dragging)
                return;

            Relayout();
            SetContentX(TargetX(_currentIndex), true);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            _horizontalVelocity = 0f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag())
                return;

            KillSnap();
            _activeMoveHandle.TryCancel();
            _dragging = true;
            _horizontalVelocity = 0f;
            _dragStartContentX = content.anchoredPosition.x;
            _dragStartLocalX = ScreenToLocalX(eventData.position, eventData);
            _lastLocalX = _dragStartLocalX;
            _lastTime = Time.unscaledTime;
            CacheActiveTargets();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || !CanDrag())
                return;

            var localX = ScreenToLocalX(eventData.position, eventData);
            var now = Time.unscaledTime;
            var dt = now - _lastTime;
            if (dt > 0.0001f)
                _horizontalVelocity = (localX - _lastLocalX) / dt;

            _lastLocalX = localX;
            _lastTime = now;

            var targetX = ApplyRubberBand(_dragStartContentX + (localX - _dragStartLocalX));
            SetContentX(targetX, true);
            SyncActiveToContentProgress();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
                return;

            _dragging = false;
            SelectPage(ResolveReleaseIndex(), true);
        }
        
        [Button]
        public void SelectPage(SwipePageKind kind, bool animated = true)
        {
            for (var i = 0; i < pages.Length; i++)
            {
                if (pages[i] != null && pages[i].PageKind == kind)
                {
                    SelectPage(i, animated);
                    return;
                }
            }
        }

        public void SelectPage(int index, bool animated = true)
        {
            if (pages == null || pages.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, pages.Length - 1);
            Relayout();

            var changed = index != _currentIndex;
            _currentIndex = index;
            ApplyTabVisuals(index, !animated);

            if (animated && Application.isPlaying)
                AnimateTo(index);
            else
                SetContentX(TargetX(index), true);

            if (!changed)
                return;

            OnPageChanged?.Invoke(index);
            if (HasPage(index))
                OnPageKindChanged?.Invoke(pages[index].PageKind);
        }

        [Button("Relayout To Index 1")]
        [ContextMenu("Relayout To Index 1")]
        public void RelayoutToIndex1()
        {
            _dragging = false;
            KillSnap();
            ResolvePageSizeFromScreenRatio();
            ApplyPageLayout(_scaledPageWidth, _scaledPageHeight);

            var homeIndex = pages == null || pages.Length == 0
                ? 0
                : Mathf.Clamp(1, 0, pages.Length - 1);
            _currentIndex = homeIndex;
            ApplyTabVisuals(homeIndex, true);
            SetContentX(TargetX(homeIndex), true);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                MarkLayoutDirty();
#endif
        }

        public void Relayout()
        {
            ResolvePageSizeFromScreenRatio();
            ApplyPageLayout(_scaledPageWidth, _scaledPageHeight);
        }

        private void ResolvePageSizeFromScreenRatio()
        {
            var designWidth = PageWidth;
            var designHeight = PageHeight;
            _scaledPageWidth = designWidth;
            _scaledPageHeight = designHeight;
            _screenSize = ReadScreenSize();

            var designRatio = designWidth / designHeight;
            var screenRatio = _screenSize.x / Mathf.Max(1f, _screenSize.y);
            if (Mathf.Abs(designRatio - screenRatio) <= 0.01f)
                return;

            var layoutSpace = ReadLayoutSpace();
            _scaledPageWidth = layoutSpace.x;
            _scaledPageHeight = layoutSpace.y;
        }

        private void ApplyPageLayout(float width, float height)
        {
            if (viewport == null || content == null || pages == null)
                return;

            content.anchorMin = new Vector2(0f, 0.5f);
            content.anchorMax = new Vector2(0f, 0.5f);
            content.pivot = new Vector2(0f, 0.5f);
            content.sizeDelta = new Vector2(width * pages.Length, height);

            for (var i = 0; i < pages.Length; i++)
            {
                var page = pages[i];
                if (page == null)
                    continue;

                page.CacheRect();
                var rect = page.Rect;
                if (rect == null)
                    continue;

                page.gameObject.SetActive(true);
                rect.SetParent(content, false);
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = new Vector2(i * width, 0f);
            }
        }

        private Vector2 ReadScreenSize()
        {
#if UNITY_EDITOR
            var gameView = UnityEditor.Handles.GetMainGameViewSize();
            if (gameView.x > 1f && gameView.y > 1f)
                return gameView;
#endif
            return ReadLayoutSpace();
        }

        private Vector2 ReadLayoutSpace()
        {
            Canvas.ForceUpdateCanvases();

            if (viewport != null)
            {
                var viewportSize = viewport.rect.size;
                if (viewportSize.x > 1f && viewportSize.y > 1f)
                    return viewportSize;
            }

            var canvas = viewport != null
                ? viewport.GetComponentInParent<Canvas>()
                : GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var root = canvas.transform as RectTransform;
                if (root != null && root.rect.width > 1f && root.rect.height > 1f)
                    return root.rect.size;
            }

            return new Vector2(PageWidth, PageHeight);
        }

        private void AnimateTo(int index)
        {
            KillSnap();
            var target = TargetX(index);
            _snapTween = content
                .DOAnchorPosX(target, snapDuration)
                .SetEase(snapEase)
                .SetUpdate(true)
                .OnKill(() => _snapTween = null);
        }

        private int ResolveReleaseIndex()
        {
            var pageWidth = ScaledPageWidth;
            if (pageWidth <= 1f)
                return _currentIndex;

            var progress = -content.anchoredPosition.x / pageWidth;
            var nearest = Mathf.Clamp(Mathf.RoundToInt(progress), 0, pages.Length - 1);
            var offsetFromCurrent = progress - _currentIndex;
            var target = nearest;

            if (_horizontalVelocity > flickVelocity)
                target = _currentIndex - 1;
            else if (_horizontalVelocity < -flickVelocity)
                target = _currentIndex + 1;
            else if (offsetFromCurrent >= changePageThreshold)
                target = _currentIndex + 1;
            else if (offsetFromCurrent <= -changePageThreshold)
                target = _currentIndex - 1;

            return Mathf.Clamp(target, 0, pages.Length - 1);
        }

        private float ApplyRubberBand(float x)
        {
            var minX = TargetX(pages.Length - 1);
            const float maxX = 0f;

            if (x > maxX)
                return maxX + (x - maxX) * rubberBand;
            if (x < minX)
                return minX + (x - minX) * rubberBand;

            return x;
        }

        private float TargetX(int index)
        {
            return -index * ScaledPageWidth;
        }

        public float PageWidth => Mathf.Max(1f, pageWidth);
        public float PageHeight => Mathf.Max(1f, pageHeight);
        public Vector2 ScreenSize => _screenSize;
        public float ScaledPageWidth => Mathf.Max(1f, _scaledPageWidth);
        public float ScaledPageHeight => Mathf.Max(1f, _scaledPageHeight);

        public void SetPageSize(float width, float height)
        {
            pageWidth = Mathf.Max(1f, width);
            pageHeight = Mathf.Max(1f, height);
            Relayout();
            SetContentX(TargetX(_currentIndex), true);
        }

        private void SetContentX(float x, bool instant)
        {
            if (content == null)
                return;

            if (instant)
                KillSnap();

            var pos = content.anchoredPosition;
            pos.x = x;
            content.anchoredPosition = pos;
        }

        private float ScreenToLocalX(Vector2 screenPosition, PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                viewport,
                screenPosition,
                eventData.pressEventCamera,
                out var local);

            return local.x;
        }

        private void BindTabs()
        {
            EnsureActiveIndicator();

            if (tabs == null)
                return;

            foreach (var tab in tabs)
                tab?.Initialize(this);
        }

        private void ApplyTabVisuals(int selectedIndex, bool instant)
        {
            if (tabs == null || !HasPage(selectedIndex))
                return;

            var kind = pages[selectedIndex].PageKind;
            SwipeTabItem selectedTab = null;
            foreach (var tab in tabs)
            {
                if (tab == null)
                    continue;

                var selected = tab.PageKind == kind;
                if (selected)
                    selectedTab = tab;

                tab.ApplyVisualState(selected, instant);
            }

            MoveActiveToTab(selectedTab, instant);
        }

        private SwipeTabItem FindTabForPage(int pageIndex)
        {
            if (!HasPage(pageIndex) || tabs == null)
                return null;

            var kind = pages[pageIndex].PageKind;
            foreach (var tab in tabs)
            {
                if (tab != null && tab.PageKind == kind)
                    return tab;
            }

            return null;
        }

        private void CacheActiveTargets()
        {
            if (pages == null || pages.Length == 0)
            {
                _activeTargetsByPage = null;
                return;
            }

            Canvas.ForceUpdateCanvases();
            if (_activeTargetsByPage == null || _activeTargetsByPage.Length != pages.Length)
                _activeTargetsByPage = new Vector2[pages.Length];

            for (var i = 0; i < pages.Length; i++)
            {
                var target = TryGetActiveTarget(FindTabForPage(i));
                _activeTargetsByPage[i] = target ?? Vector2.zero;
            }
        }

        private void SyncActiveToContentProgress()
        {
            if (activeIndicator == null || pages == null || pages.Length == 0)
                return;

            var pageWidth = ScaledPageWidth;
            if (pageWidth <= 1f)
                return;

            if (_activeTargetsByPage == null || _activeTargetsByPage.Length != pages.Length)
                CacheActiveTargets();

            if (_activeTargetsByPage == null)
                return;

            var progress = Mathf.Clamp(-content.anchoredPosition.x / pageWidth, 0f, pages.Length - 1);
            var fromIndex = Mathf.FloorToInt(progress);
            var toIndex = Mathf.Min(fromIndex + 1, pages.Length - 1);
            var t = progress - fromIndex;
            var target = Vector2.Lerp(_activeTargetsByPage[fromIndex], _activeTargetsByPage[toIndex], t);

            _activeMoveHandle.TryCancel();
            activeIndicator.anchoredPosition = target;
        }

        private Vector2? TryGetActiveTarget(SwipeTabItem tab)
        {
            if (activeIndicator == null || tab == null)
                return null;

            var parent = activeIndicator.parent as RectTransform;
            if (parent == null)
                return null;

            var tabRect = tab.transform as RectTransform;
            var worldCenter = tabRect != null
                ? tabRect.TransformPoint(tabRect.rect.center)
                : tab.transform.position;
            return (Vector2)parent.InverseTransformPoint(worldCenter);
        }

        private void EnsureActiveIndicator()
        {
            if (activeIndicator != null || tabs == null)
                return;

            foreach (var tab in tabs)
            {
                if (tab == null)
                    continue;

                var found = tab.transform.parent != null
                    ? tab.transform.parent.Find("Active")
                    : null;
                if (found is RectTransform rect)
                {
                    activeIndicator = rect;
                    return;
                }
            }
        }

        private void MoveActiveToTab(SwipeTabItem tab, bool instant)
        {
            Canvas.ForceUpdateCanvases();
            var target = TryGetActiveTarget(tab);
            if (!target.HasValue)
                return;

            _activeMoveHandle.TryCancel();
            if (instant || activeMoveDuration <= 0f)
            {
                activeIndicator.anchoredPosition = target.Value;
                return;
            }

            _activeMoveHandle = LMotion.Create(activeIndicator.anchoredPosition, target.Value, activeMoveDuration)
                .WithEase(activeMoveEase)
                .BindToAnchoredPosition(activeIndicator);
        }

        private void EnsureViewportGraphic()
        {
            if (viewport == null)
                viewport = transform as RectTransform;

            if (viewport == null)
                return;

            if (viewport.GetComponent<Graphic>() != null)
                return;

            var image = viewport.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
        }

        private bool CanDrag()
        {
            return viewport != null && content != null && pages != null && pages.Length > 0;
        }

        private bool HasPage(int index)
        {
            return pages != null && index >= 0 && index < pages.Length && pages[index] != null;
        }

        private void KillSnap()
        {
            if (_snapTween == null)
                return;

            _snapTween.Kill();
            _snapTween = null;
        }

        private void OnValidate()
        {
            pageWidth = Mathf.Max(1f, pageWidth);
            pageHeight = Mathf.Max(1f, pageHeight);

            if (!Application.isPlaying || !_initialized)
                return;

            Relayout();
            SetContentX(TargetX(_currentIndex), true);
        }

#if UNITY_EDITOR
        private void MarkLayoutDirty()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (content != null)
                UnityEditor.EditorUtility.SetDirty(content);

            if (pages == null)
                return;

            foreach (var page in pages)
            {
                if (page != null)
                    UnityEditor.EditorUtility.SetDirty(page);
            }
        }
#endif

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            _activeMoveHandle.TryCancel();
            KillSnap();
        }
    }
}
