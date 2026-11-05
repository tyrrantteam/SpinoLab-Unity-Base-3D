using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.UI.SwipeHub
{
    /// <summary>
    /// Bottom-bar tab that asks <see cref="SwipePageHub"/> to slide to a page.
    /// </summary>
    public class SwipeTabItem : MonoBehaviour
    {
        [SerializeField] private SwipePageKind pageKind = SwipePageKind.Home;
        [SerializeField] private Button button;
        [SerializeField] private Graphic icon;
        [SerializeField] private Graphic label;
        [SerializeField] private float selectedIconScale = 1.5f;
        [SerializeField] private float unselectedIconScale = 1f;
        [SerializeField] private Vector2 selectedIconOffset;
        [SerializeField] private float selectedLabelScale = 1.15f;
        [SerializeField] private float unselectedLabelScale = 0.8f;
        [SerializeField] private float animDuration = 0.2f;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private SwipePageHub _hub;
        private MotionHandle _iconScaleHandle;
        private MotionHandle _iconMoveHandle;
        private MotionHandle _labelScaleHandle;
        private Vector2 _iconRestPosition;
        private bool _iconRestCached;

        public SwipePageKind PageKind => pageKind;

        public void Initialize(SwipePageHub hub)
        {
            _hub = hub;

            if (button == null)
                button = GetComponent<Button>();

            if (button == null)
                return;

            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
            CacheIconRestPosition();
        }

        public void SetPageKind(SwipePageKind kind)
        {
            pageKind = kind;
        }

        public void ApplyVisualState(bool selected, bool instant = false)
        {
            KillTweens();

            var iconScale = selected ? selectedIconScale : unselectedIconScale;
            var labelScale = selected ? selectedLabelScale : unselectedLabelScale;
            var duration = instant ? 0f : animDuration;

            if (icon != null)
            {
                CacheIconRestPosition();
                var iconRect = icon.rectTransform;
                var iconPos = selected ? _iconRestPosition + selectedIconOffset : _iconRestPosition;
                AnimateScale(iconRect, iconScale, duration, ref _iconScaleHandle, null);
                AnimateAnchoredPosition(iconRect, iconPos, duration, ref _iconMoveHandle);
            }

            if (label == null)
                return;

            var labelObject = label.gameObject;
            var labelRect = label.rectTransform;

            if (selected)
            {
                labelObject.SetActive(true);
                AnimateScale(labelRect, labelScale, duration, ref _labelScaleHandle, null);
                return;
            }

            if (instant || duration <= 0f)
            {
                labelRect.localScale = Vector3.one * labelScale;
                labelObject.SetActive(false);
                return;
            }

            AnimateScale(labelRect, labelScale, duration, ref _labelScaleHandle, () =>
            {
                if (labelObject != null)
                    labelObject.SetActive(false);
            });
        }

        private void AnimateScale(
            RectTransform target,
            float scale,
            float duration,
            ref MotionHandle handle,
            System.Action onComplete)
        {
            var end = Vector3.one * scale;
            if (duration <= 0f)
            {
                target.localScale = end;
                onComplete?.Invoke();
                return;
            }

            var motion = LMotion.Create(target.localScale, end, duration).WithEase(ease);
            if (onComplete != null)
                motion = motion.WithOnComplete(onComplete);

            handle = motion.BindToLocalScale(target);
        }

        private void AnimateAnchoredPosition(
            RectTransform target,
            Vector2 position,
            float duration,
            ref MotionHandle handle)
        {
            if (duration <= 0f)
            {
                target.anchoredPosition = position;
                return;
            }

            handle = LMotion.Create(target.anchoredPosition, position, duration)
                .WithEase(ease)
                .BindToAnchoredPosition(target);
        }

        private void CacheIconRestPosition()
        {
            if (_iconRestCached || icon == null)
                return;

            _iconRestPosition = icon.rectTransform.anchoredPosition;
            _iconRestCached = true;
        }

        private void OnClicked()
        {
            if (_hub != null)
                _hub.SelectPage(pageKind);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClicked);

            KillTweens();
        }

        private void KillTweens()
        {
            _iconScaleHandle.TryCancel();
            _iconMoveHandle.TryCancel();
            _labelScaleHandle.TryCancel();
        }
    }
}
