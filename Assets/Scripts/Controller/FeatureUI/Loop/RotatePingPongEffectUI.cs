using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Xoay qua lại (ping-pong) liên tục giữa MinAngle và MaxAngle.
    /// Ví dụ: MinAngle = -30, MaxAngle = 30 → lắc trái phải.
    /// Duration = thời gian cho 1 chiều (MinAngle → MaxAngle).
    /// </summary>
    public class RotatePingPongEffectUI : EffectBaseLoop
    {
        [Title("Rotate PingPong Config")]
        [LabelText("Max Angle")]
        public float maxAngle = 30f;

        [LabelText("Min Angle")]
        public float minAngle = -30f;

        [LabelText("Ease")]
        public Ease ease = Ease.InOutSine;

        private MotionHandle _handle;
        private Vector3      _initialRotation;

        protected override void PrepareInitialState()
        {
            // Lưu rotation gốc và snap Z về minAngle (điểm bắt đầu) ngay khi enable (trước delay)
            _initialRotation = transform.localEulerAngles;
            transform.localEulerAngles = new Vector3(_initialRotation.x, _initialRotation.y, minAngle);
        }

        public override void Play()
        {
            _handle.TryCancel();
            _handle = LMotion.Create(minAngle, maxAngle, duration)
                .WithEase(ease)
                .WithLoops(LoopCount, LoopType.Yoyo)
                .BindToLocalEulerAnglesZ(transform);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localEulerAngles = _initialRotation;
        }
    }
}
