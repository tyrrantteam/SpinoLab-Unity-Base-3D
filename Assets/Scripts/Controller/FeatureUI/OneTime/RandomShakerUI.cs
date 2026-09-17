using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class RandomShakerUI : EffectBase
    {
        [LabelText("Min Angle")]
        public float minAngle = -10f;

        [LabelText("Max Angle")]
        public float maxAngle = 10f;

        [LabelText("Min Duration")]
        public float minDuration = 0.4f;

        [LabelText("Max Duration")]
        public float maxDuration = 0.8f;

        private MotionHandle _handle;

        public override void Play()
        {
            _handle.TryCancel();
            float avgDuration = (minDuration + maxDuration) * 0.5f;
            float strength = Mathf.Max(Mathf.Abs(minAngle), Mathf.Abs(maxAngle));
            _handle = LMotion.Shake.Create(0f, strength, avgDuration)
                .WithLoops(-1)
                .BindToLocalEulerAnglesZ(transform);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localEulerAngles = Vector3.zero;
        }
    }
}
