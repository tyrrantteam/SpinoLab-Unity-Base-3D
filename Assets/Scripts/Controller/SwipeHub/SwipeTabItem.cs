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

        [Header("Icon Bounce Settings")]
        [Tooltip("Bật hiệu ứng nhún nảy (bounce): vọt lên cao hơn -> nhún nhẹ xuống -> nảy êm ru lên lại đích.")]
        [SerializeField] private bool useIconBounce = true;
        [Tooltip("Thời gian toàn bộ chuyển động bounce nhún nảy (giây). 0.35s - 0.38s cho cảm giác đàn hồi tự nhiên mượt mà nhất.")]
        [SerializeField] private float bounceDuration = 0.36f;
        [Tooltip("Hệ số suy giảm lực nảy (càng nhỏ thì vọt lên càng cao, mặc định 1.4 đẩy cao ~149%).")]
        [SerializeField, Range(1.0f, 2.5f)] private float bounceDecayPower = 1.4f;
        [Tooltip("Bật nếu muốn tự vẽ AnimationCurve thủ công thay cho công thức vật lý lò xo.")]
        [SerializeField] private bool useCustomCurve = false;
        [SerializeField] private AnimationCurve customBounceCurve;

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
            var activeDuration = (selected && useIconBounce && duration > 0f)
                ? (bounceDuration > 0f ? bounceDuration : duration)
                : duration;

            if (icon != null)
            {
                CacheIconRestPosition();
                var iconRect = icon.rectTransform;
                var iconPos = selected ? _iconRestPosition + selectedIconOffset : _iconRestPosition;
                AnimateScale(iconRect, iconScale, activeDuration, ref _iconScaleHandle, null);

                if (selected && useIconBounce && duration > 0f && selectedIconOffset != Vector2.zero)
                {
                    AnimateIconBounceUp(iconRect, iconPos, activeDuration);
                }
                else
                {
                    AnimateAnchoredPosition(iconRect, iconPos, duration, ref _iconMoveHandle);
                }
            }

            if (label == null)
                return;

            var labelObject = label.gameObject;
            var labelRect = label.rectTransform;

            if (selected)
            {
                labelObject.SetActive(true);
                AnimateScale(labelRect, labelScale, activeDuration, ref _labelScaleHandle, null);
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

        private void AnimateIconBounceUp(
            RectTransform target,
            Vector2 targetPosition,
            float duration)
        {
            if (target == null) return;

            Vector2 startPos = target.anchoredPosition;

            // Nếu đang ở gần đích (ví dụ click lại chính tab đang chọn), tạo đà bật nhẹ từ dưới lên
            if (Vector2.Distance(startPos, targetPosition) < 0.5f)
            {
                startPos = targetPosition - (selectedIconOffset != Vector2.zero ? selectedIconOffset * 0.4f : new Vector2(0f, 12f));
            }

            _iconMoveHandle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.Linear)
                .Bind(t =>
                {
                    if (target == null) return;
                    float p = EvaluateBounceProgress(t);
                    target.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPosition, p);
                });
        }

        private float EvaluateBounceProgress(float t)
        {
            if (useCustomCurve && customBounceCurve != null && customBounceCurve.length > 0)
            {
                return customBounceCurve.Evaluate(t);
            }

            t = Mathf.Clamp01(t);
            // Công thức sóng điều hòa giảm chấn (damped harmonic oscillator):
            // Sóng cos(2.5 * pi * t) kết hợp độ suy giảm lũy thừa tạo quỹ đạo:
            // - t = 0: p = 0
            // - t = 0.40: p ~ 1.49 (đẩy lên rất cao, vượt đích ~49%)
            // - t = 0.80: p ~ 0.905 (nhún xuống nhẹ lọt dưới đích ~9.5%)
            // - t = 1.0: p = 1.00 (lên lại và dừng êm ru, vận tốc & gia tốc cập bến = 0, tuyệt đối không bị khựng)
            float power = Mathf.Max(bounceDecayPower, 0.5f);
            float decay = Mathf.Pow(1f - t, power);
            return 1f - decay * Mathf.Cos(2.5f * Mathf.PI * t);
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
