using JinGroup.Common.Effect;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class ScaleEffectUi : EffectBase
    {
        [LabelText("Duration")]
        public float duration = 1f;

        [LabelText("From Scale")]
        public Vector3 fromScale = Vector3.zero;

        [LabelText("To Scale")]
        public Vector3 toScale = Vector3.one;

        [LabelText("Scale Curve")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private MotionHandle _handle;

        public override void Play()
        {
            _handle.TryCancel();
            transform.localScale = fromScale;

            _handle = LMotion.Create(0f, 1f, duration)
                .WithEase(scaleCurve)
                .Bind(t => transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t));
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localScale = toScale;
        }
    }
}
