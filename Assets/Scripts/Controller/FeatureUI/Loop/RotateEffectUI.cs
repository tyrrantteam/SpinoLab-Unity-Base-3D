using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public enum RotateDirection
    {
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// Xoay tròn liên tục theo một chiều. Duration = thời gian cho 1 vòng 360°.
    /// </summary>
    public class RotateEffectUI : EffectBaseLoop
    {
        [Title("Rotate Config")]
        [LabelText("Direction"), EnumToggleButtons]
        public RotateDirection direction = RotateDirection.Clockwise;

        private MotionHandle _handle;
        private Vector3      _initialRotation;

        protected override void PrepareInitialState()
        {
            // Lưu rotation gốc và reset Z về 0 ngay khi enable (trước delay)
            _initialRotation = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(_initialRotation.x, _initialRotation.y, 0f);
        }

        public override void Play()
        {
            _handle.TryCancel();
            float endAngle = direction == RotateDirection.Clockwise ? -360f : 360f;
            _handle = LMotion.Create(0f, endAngle, duration)
                .WithEase(Ease.Linear)
                .WithLoops(LoopCount, LoopType.Restart)
                .BindToLocalEulerAnglesZ(transform);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localEulerAngles = _initialRotation;
        }
    }
}
