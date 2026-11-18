using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Scale to nhỏ liên tục theo kiểu "thở" (Yoyo loop).
    /// </summary>
    public class BreathScaleEffectUI : EffectBaseLoop
    {
        [Title("Breath Scale Config")]
        [LabelText("Max Scale")]
        public Vector3 maxScale = new Vector3(1.2f, 1.2f, 1.2f);

        [LabelText("Min Scale")]
        public Vector3 minScale = new Vector3(0.9f, 0.9f, 0.9f);

        [LabelText("Ease")]
        public Ease ease = Ease.InOutSine;

        private MotionHandle _handle;
        private Vector3      _initialScale;
        private bool         _isCaptured;

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
            transform.localScale = minScale;
        }

        public override void Play()
        {
            _handle.TryCancel();
            _handle = LMotion.Create(minScale, maxScale, duration)
                .WithEase(ease)
                .WithLoops(LoopCount, LoopType.Yoyo)
                .BindToLocalScale(transform);
        }

        public override void CancelMotion()
        {
            _handle.TryCancel();
        }

        public override void Stop()
        {
            _handle.TryCancel();
            if (_isCaptured)
                transform.localScale = _initialScale;
        }
    }
}
