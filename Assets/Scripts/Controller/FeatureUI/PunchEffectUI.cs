using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public enum PunchType
    {
        Scale,
        Position,
        Rotation
    }

    public class PunchEffectUI : EffectBase
    {
        [LabelText("Punch Type"), EnumToggleButtons]
        public PunchType punchType = PunchType.Scale;

        [LabelText("Strength")]
        public Vector3 strength = new Vector3(0.2f, 0.2f, 0f);

        [LabelText("Duration")]
        public float duration = 0.4f;

        [LabelText("Vibrato")]
        public int vibrato = 10;

        [LabelText("Elasticity"), Range(0f, 1f)]
        public float elasticity = 1f;

        private RectTransform _rt;
        private MotionHandle _handle;
        private Vector3 _initialScale;
        private Vector2 _initialPosition;
        private Vector3 _initialRotation;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public override void Play()
        {
            _handle.TryCancel();

            float dampingRatio = 1f - elasticity;

            switch (punchType)
            {
                case PunchType.Scale:
                    _initialScale = transform.localScale;
                    _handle = LMotion.Punch.Create(_initialScale, strength, duration)
                        .WithFrequency(vibrato)
                        .WithDampingRatio(dampingRatio)
                        .BindToLocalScale(transform);
                    break;

                case PunchType.Position:
                    RectTransform rt = RT;
                    _initialPosition = rt.anchoredPosition;
                    _handle = LMotion.Punch.Create(_initialPosition, (Vector2)strength, duration)
                        .WithFrequency(vibrato)
                        .WithDampingRatio(dampingRatio)
                        .BindToAnchoredPosition(rt);
                    break;

                case PunchType.Rotation:
                    _initialRotation = transform.localEulerAngles;
                    _handle = LMotion.Punch.Create(_initialRotation, strength, duration)
                        .WithFrequency(vibrato)
                        .WithDampingRatio(dampingRatio)
                        .BindToLocalEulerAngles(transform);
                    break;
            }
        }

        public override void Stop()
        {
            _handle.TryCancel();

            switch (punchType)
            {
                case PunchType.Scale:
                    transform.localScale = _initialScale;
                    break;
                case PunchType.Position:
                    RT.anchoredPosition = _initialPosition;
                    break;
                case PunchType.Rotation:
                    transform.localEulerAngles = _initialRotation;
                    break;
            }
        }

        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());
    }
}
