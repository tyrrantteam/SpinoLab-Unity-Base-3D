using JinGroup.Common.Effect;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Hiệu ứng Scale Squash & Stretch dạng lặp (Loop Effect).
    /// Cho phép config scale (rest, squash, stretch hoặc custom curves)
    /// và delay giữa các chu kỳ lặp lại.
    /// </summary>
    public class ScaleSquashAndStretchLoopEffectUI : EffectBaseLoop
    {
        [Title("Scale Config")]
        [LabelText("Rest Scale")]
        [Tooltip("Scale tại trạng thái nghỉ.")]
        public Vector3 restScale = Vector3.one;

        [HideIf("useCustomCurves")]
        [LabelText("Squash Scale (Min/Max Ratio)")]
        [Tooltip("Tỉ lệ scale khi bẹp (thường X nở ngang > 1, Y xẹp xuống < 1). Tương đối theo Rest Scale.")]
        public Vector3 squashScale = new Vector3(1.2f, 0.8f, 1f);

        [HideIf("useCustomCurves")]
        [LabelText("Stretch Scale (Min/Max Ratio)")]
        [Tooltip("Tỉ lệ scale khi giãn (thường X co lại < 1, Y vươn cao > 1). Tương đối theo Rest Scale.")]
        public Vector3 stretchScale = new Vector3(0.85f, 1.2f, 1f);

        [HideIf("useCustomCurves")]
        [LabelText("Bounce Timing Curve")]
        [Tooltip("Curve điều khiển nhịp nén và nảy (-1 là max squash, 0 là rest, 1 là max stretch).")]
        public AnimationCurve bounceCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.22f, -1f),
            new Keyframe(0.52f, 1f),
            new Keyframe(0.78f, -0.2f),
            new Keyframe(1f, 0f));

        [Title("Advanced Curves")]
        [LabelText("Use Custom Curves")]
        [Tooltip("Bật nếu muốn tùy biến chi tiết đường cong scale cho từng trục X, Y, Z thay vì dùng Squash/Stretch Scale.")]
        public bool useCustomCurves = false;

        [ShowIf("useCustomCurves")]
        [LabelText("Progress Curve")]
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [ShowIf("useCustomCurves")]
        [LabelText("Scale X Curve")]
        public AnimationCurve scaleXCurve = new(
            new Keyframe(0f, 1f),
            new Keyframe(0.22f, 1.25f),
            new Keyframe(0.52f, 0.85f),
            new Keyframe(0.78f, 1.05f),
            new Keyframe(1f, 1f));

        [ShowIf("useCustomCurves")]
        [LabelText("Scale Y Curve")]
        public AnimationCurve scaleYCurve = new(
            new Keyframe(0f, 1f),
            new Keyframe(0.22f, 0.75f),
            new Keyframe(0.52f, 1.2f),
            new Keyframe(0.78f, 0.95f),
            new Keyframe(1f, 1f));

        [ShowIf("useCustomCurves")]
        [LabelText("Scale Z Curve")]
        public AnimationCurve scaleZCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Title("Loop Delay")]
        [LabelText("Delay Between Loops")]
        [Tooltip("Khoảng thời gian nghỉ (tính bằng giây) sau mỗi lần diễn xong chu kỳ trước khi diễn tiếp lần sau.")]
        public float delayBetweenLoops = 1f;

        private MotionHandle _motionHandle;
        private MotionHandle _delayHandle;
        private Vector3      _initialScale;
        private bool         _isCaptured;
        private int          _currentLoop;

        protected override void Awake()
        {
            CaptureInitialState();
            base.Awake();
        }

        private void CaptureInitialState()
        {
            if (!_isCaptured)
            {
                _initialScale = transform.localScale == Vector3.zero ? Vector3.one : transform.localScale;
                _isCaptured = true;
            }
        }

        public override void PrepareInitialState()
        {
            CaptureInitialState();
            ApplyScale(0f);
        }

        public override void Play()
        {
            CancelMotion();
            _currentLoop = 0;
            PlayCycle();
        }

        private void PlayCycle()
        {
            CancelMotion();
            ApplyScale(0f);

            if (duration <= 0f)
            {
                ApplyScale(1f);
                OnCycleCompleted();
                return;
            }

            _motionHandle = LMotion.Create(0f, 1f, duration)
                .WithOnComplete(OnCycleCompleted)
                .Bind(ApplyScale);
        }

        private void OnCycleCompleted()
        {
            ApplyScale(1f);
            _currentLoop++;

            // Kiểm tra số lần lặp nếu không phải infiniteLoop
            if (!IsInfiniteLoopValid())
            {
                return;
            }

            // Delay giữa các chu kỳ lặp
            if (delayBetweenLoops > 0f)
            {
                _delayHandle = LMotion.Create(0f, 1f, delayBetweenLoops)
                    .WithOnComplete(PlayCycle)
                    .RunWithoutBinding();
            }
            else
            {
                PlayCycle();
            }
        }

        private bool IsInfiniteLoopValid()
        {
            // EffectBaseLoop: LoopCount = -1 nếu infiniteLoop = true, ngược lại là loopCount
            if (LoopCount < 0) return true;
            return _currentLoop < LoopCount;
        }

        public override void CancelMotion()
        {
            _motionHandle.TryCancel();
            _delayHandle.TryCancel();
        }

        public override void Stop()
        {
            CancelMotion();
            if (_isCaptured)
            {
                transform.localScale = _initialScale;
            }
            else
            {
                transform.localScale = restScale != Vector3.zero ? restScale : Vector3.one;
            }
        }

        private void ApplyScale(float t)
        {
            Vector3 baseScale = restScale != Vector3.zero ? restScale : (_isCaptured ? _initialScale : Vector3.one);

            if (useCustomCurves)
            {
                float p = progressCurve != null ? progressCurve.Evaluate(t) : t;
                float sx = scaleXCurve != null ? scaleXCurve.Evaluate(p) : 1f;
                float sy = scaleYCurve != null ? scaleYCurve.Evaluate(p) : 1f;
                float sz = scaleZCurve != null ? scaleZCurve.Evaluate(p) : 1f;
                transform.localScale = new Vector3(baseScale.x * sx, baseScale.y * sy, baseScale.z * sz);
            }
            else
            {
                float factor = bounceCurve != null ? bounceCurve.Evaluate(t) : 0f;
                Vector3 targetSquash = new Vector3(baseScale.x * squashScale.x, baseScale.y * squashScale.y, baseScale.z * squashScale.z);
                Vector3 targetStretch = new Vector3(baseScale.x * stretchScale.x, baseScale.y * stretchScale.y, baseScale.z * stretchScale.z);

                if (factor < 0f)
                {
                    transform.localScale = Vector3.LerpUnclamped(baseScale, targetSquash, -factor);
                }
                else
                {
                    transform.localScale = Vector3.LerpUnclamped(baseScale, targetStretch, factor);
                }
            }
        }
    }
}
