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

        public override void Play()
        {
            _handle.TryCancel();
            _initialScale = transform.localScale;
            _handle = LMotion.Create(minScale, maxScale, duration)
                .WithEase(ease)
                .WithLoops(LoopCount, LoopType.Yoyo)
                .BindToLocalScale(transform);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localScale = _initialScale;
        }
    }
}
