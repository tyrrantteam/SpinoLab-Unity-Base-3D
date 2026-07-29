using JinGroup.Common.Effect;
using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Pop-in scale with independent X/Y curves for squash and stretch.
    /// Curve values are multipliers applied to <see cref="restScale"/> over normalized progress.
    /// </summary>
    public class ScaleEffectSquashAndStretch : EffectBase
    {
        [LabelText("Duration")]
        public float duration = 1f;

        [LabelText("Rest Scale")]
        public Vector3 restScale = Vector3.one;

        [LabelText("Progress Curve")]
        [Tooltip("Eases normalized time before evaluating scale curves.")]
        public AnimationCurve progressCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [LabelText("Scale X Curve")]
        [Tooltip("Horizontal scale over progress. Pair with Scale Y for squash and stretch.")]
        public AnimationCurve scaleXCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 1.25f),
            new Keyframe(0.55f, 0.92f),
            new Keyframe(0.8f, 1.05f),
            new Keyframe(1f, 1f));

        [LabelText("Scale Y Curve")]
        [Tooltip("Vertical scale over progress. When Y stretches, X usually squashes.")]
        public AnimationCurve scaleYCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.25f, 0.75f),
            new Keyframe(0.55f, 1.18f),
            new Keyframe(0.8f, 0.98f),
            new Keyframe(1f, 1f));

        [LabelText("Scale Z Curve")]
        public AnimationCurve scaleZCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        private MotionHandle _handle;

        public override void Play()
        {
            _handle.TryCancel();
            ApplyScale(0f);

            _handle = LMotion.Create(0f, 1f, duration)
                .Bind(ApplyScale);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localScale = restScale;
        }

        private void ApplyScale(float t)
        {
            float progress = progressCurve.Evaluate(t);
            transform.localScale = new Vector3(
                restScale.x * scaleXCurve.Evaluate(progress),
                restScale.y * scaleYCurve.Evaluate(progress),
                restScale.z * scaleZCurve.Evaluate(progress));
        }
    }
}
