using System;
using System.Collections;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Controller.Feature;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Base.Core.Debug;
[DisallowMultipleComponent]
public class CustomVerticalLayout : MonoBehaviour
{
    [SerializeField] private float spacing;
    [SerializeField, MinValue(0.01f), Tooltip("Thời gian mỗi nhịp shift phao.")]
    private float catchUpPhaseDuration ;
    [SerializeField, MinValue(0.01f), Tooltip("Thời gian phao 3 (slot đáy) trượt xuống và biến mất.")]
    private float slot3ExitDuration = 0.35f;
    [SerializeField, Tooltip("Hệ số slide UV Rope so với khoảng phao đi xuống. 1 = cùng tốc độ.")]
    private float ropeSlideMultiplier = 1f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;
    [SerializeField] private GameObject smallFloatyPrefab;
    [SerializeField] private GameObject largeFloatyPrefab;
    [FoldoutGroup("Test Editor")]
    [SerializeField, MinValue(1)]
    private int testFromLevel = 1;
    [FoldoutGroup("Test Editor")]
    [SerializeField, MinValue(1)]
    private int testToLevel = 5;
    private readonly List<RectTransform> _items = new();
    private readonly List<MoveJob> _moveJobs = new();
    private readonly List<RectTransform> _smallPool = new();
    private readonly List<RectTransform> _largePool = new();
    private readonly List<Image> _ropes = new();
    private readonly List<RopeUvDriver> _ropeDrivers = new();
    private Material _ropeMat;
    private Coroutine _catchUpRoutine;
    private bool _isCatchingUp;
    private bool _isShifting;
    private bool _resumeCatchUpOnEnable;
    private bool _ropeSliding;
    private float _ropeElapsed;
    private float _ropeDuration;
    private float _ropeFromPx;
    private float _ropeToPx;
    private float _ropePx;

    public bool IsBusy => _isShifting || _isCatchingUp;

    private void Start()
    {
        if (!Application.isPlaying)
            return;

        PlayHomeLevelPresentation();
    }

    private void Update()
    {
        float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
        TickMoves(dt);
        TickRopeSlide(dt);
    }

    private void OnDestroy()
    {
        StopCatchUp();
        CancelMoves();
        FinishRopeSlide();
        if (_ropeMat != null)
            Destroy(_ropeMat);
    }

    private void OnEnable()
    {
        if (!_resumeCatchUpOnEnable || !Application.isPlaying)
            return;

        _resumeCatchUpOnEnable = false;
        StopCatchUp();
        _catchUpRoutine = StartCoroutine(CoResumeCatchUpAfterDisable());
    }

    private void OnDisable()
    {
        bool pauseCatchUp = _isCatchingUp || _isShifting || _moveJobs.Count > 0;
        if (pauseCatchUp)
        {
            _resumeCatchUpOnEnable = true;
            return;
        }

        StopCatchUp();
        CancelMoves();
        DespawnInactiveFloaties();
    }

    private void PlayHomeLevelPresentation()
    {
        int current = GetCurrentLevel();
        int lastShown = DataAccountPlayer.PlayerPointProcessData != null
            ? DataAccountPlayer.PlayerPointProcessData.lastHomeLevel
            : 0;

        if (lastShown <= 0 || current <= lastShown)
        {
            ApplyLevelsFromBase(current);
            DespawnInactiveFloaties();
            SaveLastHomeFloatyLevel(current);
            return;
        }

        int from = Mathf.Max(1, current - 1);
        ApplyLevelsFromBase(from);
        StopCatchUp();
        _catchUpRoutine = StartCoroutine(CoCatchUpToCurrentLevel(from, current, saveProgress: true));
    }

    private IEnumerator CoResumeCatchUpAfterDisable()
    {
        yield return new WaitUntil(() => !_isShifting);

        int toBottom = GetCurrentLevel();
        int fromBottom = GetDisplayedBottomLevel();
        if (fromBottom <= 0)
            fromBottom = Mathf.Max(1, toBottom - 1);

        if (fromBottom >= toBottom)
        {
            ApplyLevelsFromBase(toBottom);
            ResumeFloatingOnVisibleItems();
            DespawnInactiveFloaties();
            SaveLastHomeFloatyLevel(toBottom);
            yield break;
        }

        yield return CoCatchUpToCurrentLevel(fromBottom, toBottom, saveProgress: true, skipIntroDelay: true);
    }

    private IEnumerator CoCatchUpToCurrentLevel(int fromBottom, int toBottom, bool saveProgress, bool skipIntroDelay = false)
    {
        _isCatchingUp = true;
        if (!skipIntroDelay)
        {
            yield return null;
            WarmPools();
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            WarmPools();
        }

        for (int step = fromBottom; step < toBottom; step++)
        {
            bool done = false;
            ShiftDown(() => done = true);
            yield return new WaitUntil(() => done);
        }

        ResumeFloatingOnVisibleItems();
        DespawnInactiveFloaties();
        if (saveProgress)
            SaveLastHomeFloatyLevel(toBottom);
        StopCatchUp();
    }

    [Button("TEST Shift Down", ButtonSizes.Large), GUIColor(0f, 1f, 0f)]
    [DisableIf(nameof(IsBusy))]
    private void TestShiftDown()
    {
        ShiftDown();
    }

    [FoldoutGroup("Test Editor")]
    [Button("Test chạy tới level", ButtonSizes.Large), GUIColor(0.25f, 0.75f, 1f)]
    [DisableIf(nameof(IsBusy))]
    [InfoBox("Bật Play trên scene rồi bấm. Không ghi lastHomeFloatyLevel.")]
    private void TestCatchUpToLevel()
    {
        if (!Application.isPlaying)
        {
            GameDebug.LogWarning("[CustomVerticalLayout] Cần Play Mode để test catch-up phao.");
            return;
        }

        int from = Mathf.Max(1, testFromLevel);
        int to = Mathf.Max(1, testToLevel);
        if (to <= from)
        {
            ApplyLevelsFromBase(to);
            GameDebug.Log($"[CustomVerticalLayout] testToLevel ({to}) <= testFromLevel ({from}), set thẳng không chạy catch-up.");
            return;
        }

        StopCatchUp();
        CancelMoves();
        ApplyLevelsFromBase(from);
        _catchUpRoutine = StartCoroutine(CoCatchUpToCurrentLevel(from, to, saveProgress: false));
    }

    private void ShiftDown(Action onComplete = null)
    {
        if (_isShifting)
            return;

        CancelMoves();
        CollectItems();
        if (_items.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        PrepareItems();
        _isShifting = true;
        BeginRopeSlide(GetHeight(_items[0]) + Mathf.Max(0f, spacing));

        var poses = new SlotPose[_items.Count];
        for (int i = 0; i < _items.Count; i++)
            poses[i] = new SlotPose(_items[i].localPosition, _items[i].localScale);

        int pending = 0;
        void MarkDone()
        {
            pending--;
            if (pending > 0)
                return;

            _isShifting = false;
            onComplete?.Invoke();
            if (!_isCatchingUp)
                DespawnInactiveFloaties();
        }

        int lastIndex = _items.Count - 1;
        var spawned = SpawnIncoming(poses[0], MarkDone);
        RectTransform promoted = null;

        for (int i = 0; i < lastIndex; i++)
        {
            bool toSlot3 = i + 1 == lastIndex;
            var target = toSlot3
                ? new SlotPose(poses[lastIndex].LocalPosition, poses[lastIndex].LocalScale)
                : poses[i + 1];
            _items[i].SetAsLastSibling();
            if (toSlot3)
                promoted = AnimatePromoteToLarge(_items[i], _items[lastIndex], target, MarkDone);
            else
                AnimateToPose(_items[i], target, false, MarkDone);
            pending++;
        }

        _items[lastIndex].SetAsLastSibling();
        AnimateExit(_items[lastIndex], poses[lastIndex], MarkDone);
        pending++;

        promoted?.SetAsLastSibling();
        spawned?.SetAsLastSibling();
        if (spawned != null)
            pending++;
    }
    private RectTransform SpawnIncoming(SlotPose topPose, Action onComplete)
    {
        var spawned = TakeFloaty(FloatyKind.Small, _items[0]);
        if (spawned == null)
            return null;

        float enterDistance = GetHeight(_items[0]) + Mathf.Max(0f, spacing);
        spawned.localPosition = topPose.LocalPosition + Vector3.up * enterDistance;
        spawned.localScale = topPose.LocalScale;
        spawned.gameObject.SetActive(true);
        SetItemLevel(spawned, GetNextSpawnLevel());
        AnimateToPose(spawned, topPose, false, onComplete);
        return spawned;
    }
    private void AnimateExit(RectTransform item, SlotPose fromPose, Action onComplete)
    {
        float exitDistance = GetHeight(item) + Mathf.Max(0f, spacing);
        var exitPose = new SlotPose(fromPose.LocalPosition + Vector3.down * exitDistance, fromPose.LocalScale);
        AnimateToPose(item, exitPose, true, onComplete, 1f, 1f, slot3ExitDuration);
    }
    private RectTransform TakeFloaty(FloatyKind kind, RectTransform template)
    {
        var pool = kind == FloatyKind.Large ? _largePool : _smallPool;
        RectTransform rt = null;
        for (int i = pool.Count - 1; i >= 0; i--)
        {
            if (pool[i] == null)
            {
                pool.RemoveAt(i);
                continue;
            }

            rt = pool[i];
            pool.RemoveAt(i);
            break;
        }

        if (rt == null)
            rt = SpawnFloaty(kind == FloatyKind.Large ? largeFloatyPrefab : smallFloatyPrefab, kind, template);

        if (rt == null)
            return null;

        PrepareTakenFloaty(rt, kind, template);
        return rt;
    }
    private void WarmPools()
    {
        WarmPool(FloatyKind.Small, 3, _items.Count > 0 ? _items[0] : null);
        WarmPool(FloatyKind.Large, 2, _items.Count > 0 ? _items[_items.Count - 1] : null);
    }
    private void WarmPool(FloatyKind kind, int count, RectTransform template)
    {
        var pool = kind == FloatyKind.Large ? _largePool : _smallPool;
        while (pool.Count < count)
        {
            var rt = SpawnFloaty(kind == FloatyKind.Large ? largeFloatyPrefab : smallFloatyPrefab, kind, template);
            if (rt == null)
                break;
            Recycle(rt);
        }
    }
    private static void PrepareTakenFloaty(RectTransform rt, FloatyKind kind, RectTransform template)
    {
        rt.SetParent(rt.parent, false);
        rt.SetAsLastSibling();
        rt.localRotation = Quaternion.identity;
        if (template != null)
        {
            rt.anchorMin = template.anchorMin;
            rt.anchorMax = template.anchorMax;
            rt.pivot = template.pivot;
        }

        var item = rt.GetComponent<CustomVerticalLayoutItem>();
        if (item == null)
            item = rt.gameObject.AddComponent<CustomVerticalLayoutItem>();
        item.kind = kind;
        item.ignoreLayout = false;

        var canvasGroup = rt.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = rt.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        rt.GetComponent<FloatingEffectUI>()?.CancelMotion();
    }
    private RectTransform SpawnFloaty(GameObject prefab, FloatyKind kind, RectTransform template)
    {
        var source = prefab != null ? prefab : template != null ? template.gameObject : null;
        if (source == null)
            return null;

        var instance = Instantiate(source, transform);
        instance.name = kind == FloatyKind.Large ? "FloatyLarge" : "FloatySmall";
        instance.SetActive(false);

        var rt = instance.GetComponent<RectTransform>();
        if (rt == null)
        {
            Destroy(instance);
            return null;
        }

        PrepareTakenFloaty(rt, kind, template);
        return rt;
    }
    private void ApplyLevelsFromBase(int bottomLevel)
    {
        CollectItems();
        if (_items.Count == 0)
            return;

        PrepareItems();
        int last = _items.Count - 1;
        for (int i = 0; i < _items.Count; i++)
            SetItemLevel(_items[i], bottomLevel + (last - i));

        EnsureBottomIsLarge();
    }
    private void EnsureBottomIsLarge()
    {
        if (_items.Count == 0)
            return;

        int last = _items.Count - 1;
        var bottom = _items[last];
        var item = bottom != null ? bottom.GetComponent<CustomVerticalLayoutItem>() : null;
        if (item != null && item.kind == FloatyKind.Large)
            return;

        var large = ReplaceWithLarge(bottom);
        if (large != null)
            _items[last] = large;
    }

    private int GetNextSpawnLevel()
    {
        int topLevel = GetItemLevel(_items.Count > 0 ? _items[0] : null);
        return topLevel > 0 ? topLevel + 1 : GetCurrentLevel() + Mathf.Max(1, _items.Count);
    }

    private int GetDisplayedBottomLevel()
    {
        CollectItems();
        if (_items.Count == 0)
            return 0;

        return GetItemLevel(_items[_items.Count - 1]);
    }

    private static int GetCurrentLevel()
    {
        var data = DataAccountPlayer.PlayerPointProcessData;
        return data != null ? Mathf.Max(1, data.currentlevelShowScreen) : 1;
    }

    private static void SaveLastHomeFloatyLevel(int level)
    {
        var data = DataAccountPlayer.PlayerPointProcessData;
        if (data == null)
            return;

        data.lastHomeLevel = Mathf.Max(1, level);
        DataAccountPlayer.SavePlayerPointProcessData();
    }

    private static int GetItemLevel(RectTransform rt)
    {
        var item = rt != null ? rt.GetComponent<CustomVerticalLayoutItem>() : null;
        return item != null ? item.displayedLevel : 0;
    }

    private static void SetItemLevel(RectTransform rt, int level)
    {
        if (rt == null)
            return;

        var item = rt.GetComponent<CustomVerticalLayoutItem>();
        if (item == null)
            item = rt.gameObject.AddComponent<CustomVerticalLayoutItem>();
        item.SetDisplayedLevel(level);
    }

    private void CollectItems()
    {
        _items.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;

            if (child == null || !child.gameObject.activeSelf)
                continue;

            if (child.GetComponent<CustomVerticalLayoutItem>() == null)
                continue;

            _items.Add(child);
        }

        _items.Sort((a, b) =>
        {
            int levelA = GetItemLevel(a);
            int levelB = GetItemLevel(b);

            if (levelA > 0 && levelB > 0 && levelA != levelB)
                return levelB.CompareTo(levelA);

            return b.localPosition.y.CompareTo(a.localPosition.y);
        });
    }
    private void PrepareItems()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            var child = _items[i];
            child.GetComponent<FloatingEffectUI>()?.Stop();
            ResetAlpha(child);
        }
    }

    private static float GetHeight(RectTransform child)
    {
        return Mathf.Max(0f, child.rect.height * Mathf.Abs(child.localScale.y));
    }

    private void AnimateToPose(RectTransform child,SlotPose target,bool destroyOnComplete,Action onComplete,float fromAlpha = 1f,float toAlpha = 1f,float duration = -1f)
    {
        if (child == null)
        {
            onComplete?.Invoke();
            return;
        }
        var floatFx = child.GetComponent<FloatingEffectUI>();
        floatFx?.CancelMotion();

        CanvasGroup canvasGroup = null;
        if (!Mathf.Approximately(fromAlpha, toAlpha))
        {
            canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = child.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = fromAlpha;
        }
        _moveJobs.Add(new MoveJob
        {
            Child = child,
            FromPos = child.localPosition,
            ToPos = target.LocalPosition,
            FromScale = child.localScale,
            ToScale = target.LocalScale,
            FromAlpha = fromAlpha,
            ToAlpha = toAlpha,
            CanvasGroup = canvasGroup,
            DestroyOnComplete = destroyOnComplete,
            OnComplete = onComplete,
            FloatFx = floatFx,
            Duration = Mathf.Max(0.01f, duration > 0f ? duration : catchUpPhaseDuration)
        });
    }

    private RectTransform AnimatePromoteToLarge(RectTransform small, RectTransform largeTemplate, SlotPose target, Action onComplete)
    {
        if (small == null)
        {
            onComplete?.Invoke();
            return null;
        }

        var large = TakeFloaty(FloatyKind.Large, largeTemplate != null ? largeTemplate : small);
        if (large == null)
        {
            AnimateToPose(small, target, false, onComplete);
            return small;
        }

        int level = GetItemLevel(small);
        large.localPosition = small.localPosition;
        large.localRotation = small.localRotation;
        large.localScale = small.localScale;
        SetItemLevel(large, level);
        large.gameObject.SetActive(true);
        large.SetAsLastSibling();
        Recycle(small);

        ResetAlpha(large);
        AnimateToPose(large, target, false, onComplete, 1f, 1f);
        return large;
    }

    private void TickMoves(float deltaTime)
    {
        for (int i = _moveJobs.Count - 1; i >= 0; i--)
        {
            var job = _moveJobs[i];
            if (job.Child == null)
            {
                _moveJobs.RemoveAt(i);
                job.OnComplete?.Invoke();
                continue;
            }

            job.Elapsed += deltaTime;
            float duration = Mathf.Max(0.01f, job.Duration);
            Ease ease = _isCatchingUp ? Ease.Linear : moveEase;
            float t = EvaluateEase(Mathf.Clamp01(job.Elapsed / duration), ease);
            job.Child.localPosition = Vector3.LerpUnclamped(job.FromPos, job.ToPos, t);
            job.Child.localScale = Vector3.LerpUnclamped(job.FromScale, job.ToScale, t);
            if (job.CanvasGroup != null)
                job.CanvasGroup.alpha = Mathf.Lerp(job.FromAlpha, job.ToAlpha, t);

            if (job.Elapsed < duration)
                continue;

            job.Child.localPosition = job.ToPos;
            job.Child.localScale = job.ToScale;
            if (job.CanvasGroup != null)
                job.CanvasGroup.alpha = job.ToAlpha;

            _moveJobs.RemoveAt(i);
            FinishMoveJob(job);
        }
    }

    private void FinishMoveJob(MoveJob job)
    {
        if (job.Child == null)
        {
            job.OnComplete?.Invoke();
            return;
        }

        if (job.DestroyOnComplete)
        {
            Recycle(job.Child);
        }
        else
        {
            ResetAlpha(job.Child);
            if (!_isCatchingUp && job.FloatFx != null && job.FloatFx.isActiveAndEnabled)
                job.FloatFx.Play();
        }

        job.OnComplete?.Invoke();
    }

    private RectTransform ReplaceWithLarge(RectTransform small)
    {
        if (small == null)
            return null;

        int level = GetItemLevel(small);
        int sibling = small.GetSiblingIndex();
        Vector3 pos = small.localPosition;
        Quaternion rot = small.localRotation;

        Vector3 scale = small.localScale;

        var large = TakeFloaty(FloatyKind.Large, small);
        Recycle(small);
        if (large == null)
            return null;

        large.localPosition = pos;
        large.localRotation = rot;
        large.localScale = scale;
        large.SetSiblingIndex(sibling);
        large.gameObject.SetActive(true);
        SetItemLevel(large, level);

        var floatFx = large.GetComponent<FloatingEffectUI>();
        if (floatFx != null && floatFx.isActiveAndEnabled)
            floatFx.Play();

        return large;
    }

    private void Recycle(RectTransform child)
    {
        if (child == null)
            return;

        child.GetComponent<FloatingEffectUI>()?.CancelMotion();
        var item = child.GetComponent<CustomVerticalLayoutItem>();
        if (item != null)
            item.ignoreLayout = true;

        ResetAlpha(child);
        child.gameObject.SetActive(false);

        var pool = item != null && item.kind == FloatyKind.Large ? _largePool : _smallPool;
        if (!pool.Contains(child))
            pool.Add(child);
    }

    private void DespawnInactiveFloaties()
    {
        _smallPool.Clear();
        _largePool.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child == null || child.gameObject.activeSelf)
                continue;
            if (child.GetComponent<CustomVerticalLayoutItem>() == null)
                continue;

            Destroy(child.gameObject);
        }
    }

    private static void ResetAlpha(RectTransform child)
    {
        var canvasGroup = child != null ? child.GetComponent<CanvasGroup>() : null;
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    private static float EvaluateEase(float t, Ease ease)
    {
        return ease switch
        {
            Ease.Linear => t,
            Ease.InCubic => t * t * t,
            Ease.InOutCubic => t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,
            _ => 1f - Mathf.Pow(1f - t, 3f)
        };
    }

    private void CancelMoves()
    {
        for (int i = 0; i < _moveJobs.Count; i++)
        {
            if (_moveJobs[i].DestroyOnComplete)
                Recycle(_moveJobs[i].Child);
        }

        _moveJobs.Clear();
        FinishRopeSlide();
        _isShifting = false;
    }

    private void CollectRopes()
    {
        _ropes.Clear();
        _ropeDrivers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child == null || !child.gameObject.activeSelf)
                continue;
            if (!child.name.StartsWith("Rope"))
                continue;

            var image = child.GetComponent<Image>();
            if (image == null)
                continue;

            _ropes.Add(image);
            EnsureRopeMaterial(image);
            var driver = child.GetComponent<RopeUvDriver>();
            if (driver == null)
                driver = child.gameObject.AddComponent<RopeUvDriver>();
            _ropeDrivers.Add(driver);
        }
    }

    private void EnsureRopeMaterial(Image image)
    {
        if (image == null)
            return;

        if (_ropeMat == null)
        {
            var source = image.material != null ? image.material : image.defaultMaterial;
            _ropeMat = source != null ? new Material(source) : new Material(Shader.Find("UI/Default"));
            var tex = image.sprite != null ? image.sprite.texture : null;
            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Repeat;
                _ropeMat.mainTexture = tex;
            }
        }

        image.material = _ropeMat;
    }

    private void BeginRopeSlide(float pixelDistance)
    {
        CollectRopes();
        if (_ropes.Count == 0)
            return;

        float travel = pixelDistance * ropeSlideMultiplier;
        _ropeFromPx = _ropePx;
        _ropeToPx = _ropePx + travel;
        _ropeElapsed = 0f;
        _ropeDuration = Mathf.Max(0.01f, catchUpPhaseDuration);
        _ropeSliding = true;
        ApplyRopePx(_ropeFromPx);
    }

    private void TickRopeSlide(float deltaTime)
    {
        if (!_ropeSliding)
            return;

        _ropeElapsed += deltaTime;
        float duration = Mathf.Max(0.01f, _ropeDuration);
        Ease ease = _isCatchingUp ? Ease.Linear : moveEase;
        float t = EvaluateEase(Mathf.Clamp01(_ropeElapsed / duration), ease);
        ApplyRopePx(Mathf.LerpUnclamped(_ropeFromPx, _ropeToPx, t));

        if (_ropeElapsed < duration)
            return;

        ApplyRopePx(_ropeToPx);
        _ropeSliding = false;
    }

    private void FinishRopeSlide()
    {
        if (!_ropeSliding)
            return;

        ApplyRopePx(_ropeToPx);
        _ropeSliding = false;
    }

    private void ApplyRopePx(float pixelOffset)
    {
        _ropePx = pixelOffset;
        for (int i = 0; i < _ropes.Count; i++)
        {
            var image = _ropes[i];
            if (image == null)
                continue;

            float height = Mathf.Max(1f, image.rectTransform.rect.height);
            var driver = i < _ropeDrivers.Count ? _ropeDrivers[i] : image.GetComponent<RopeUvDriver>();
            if (driver == null)
                continue;

            driver.OffsetY = pixelOffset / height;
            image.SetVerticesDirty();
        }
    }

    private sealed class RopeUvDriver : MonoBehaviour, IMeshModifier
    {
        public float OffsetY;

        public void ModifyMesh(VertexHelper vh)
        {
            if (Mathf.Approximately(OffsetY, 0f))
                return;

            UIVertex v = default;
            int count = vh.currentVertCount;
            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                var uv = v.uv0;
                uv.y += OffsetY;
                v.uv0 = uv;
                vh.SetUIVertex(v, i);
            }
        }

        public void ModifyMesh(Mesh mesh)
        {
        }
    }

    private void StopCatchUp()
    {
        if (_catchUpRoutine != null)
        {
            StopCoroutine(_catchUpRoutine);
            _catchUpRoutine = null;
        }

        _isCatchingUp = false;
    }

    private void ResumeFloatingOnVisibleItems()
    {
        CollectItems();
        for (int i = 0; i < _items.Count; i++)
        {
            var floatFx = _items[i] != null ? _items[i].GetComponent<FloatingEffectUI>() : null;
            if (floatFx != null && floatFx.isActiveAndEnabled)
                floatFx.Play();
        }
    }

    private readonly struct SlotPose
    {
        public readonly Vector3 LocalPosition;
        public readonly Vector3 LocalScale;

        public SlotPose(Vector3 localPosition, Vector3 localScale)
        {
            LocalPosition = localPosition;
            LocalScale = localScale;
        }
    }

    private sealed class MoveJob
    {
        public RectTransform Child;
        public Vector3 FromPos;
        public Vector3 ToPos;
        public Vector3 FromScale;
        public Vector3 ToScale;
        public float FromAlpha;
        public float ToAlpha;
        public CanvasGroup CanvasGroup;
        public bool DestroyOnComplete;
        public Action OnComplete;
        public FloatingEffectUI FloatFx;
        public float Elapsed;
        public float Duration;
    }
}
