using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class ScaleEffectBounceUI : EffectBase
    {
        [LabelText("Duration")]
        public float duration = 1f;

        [LabelText("From Scale")]
        public Vector3 fromScale = Vector3.zero;

        [LabelText("To Scale")]
        public Vector3 toScale = Vector3.one;

        [LabelText("Scale Extend")]
        [Tooltip("Độ dôi ra/thụt xuống khi bounce: vượt qua toScale một đoạn = scaleExtend rồi mới hồi về toScale.")]
        public float scaleExtend = 0.1f;

        [LabelText("Scale Curve")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [LabelText("Bounce Ratio"), Range(0.1f, 0.9f)]
        [Tooltip("Tỷ lệ thời gian giữa 2 phase: từ start -> peak và từ peak -> toScale.")]
        public float bounceRatio = 0.7f;

        private MotionHandle _handle;

        public override void PrepareInitialState()
        {
            // Snap về fromScale ngay lập tức (trước khi render frame đầu tiên)
            transform.localScale = fromScale;
        }

        public override void Play()
        {
            _handle.TryCancel();
            transform.localScale = fromScale;

            if (duration <= 0f)
            {
                transform.localScale = toScale;
                return;
            }

            // Nếu scaleExtend = 0 thì diễn mượt trực tiếp như ScaleEffectUi thông thường
            if (Mathf.Approximately(scaleExtend, 0f))
            {
                _handle = LMotion.Create(fromScale, toScale, duration)
                    .WithEase(scaleCurve)
                    .BindToLocalScale(transform);
                return;
            }

            Vector3 peakScale = CalculatePeakScale();
            float phase1Duration = duration * bounceRatio;
            float phase2Duration = duration * (1f - bounceRatio);

            // Phase 1: FromScale -> PeakScale (vượt ngưỡng hoặc thụt ngưỡng theo scaleExtend)
            // Phase 2: PeakScale -> ToScale (hồi về toScale)
            _handle = LSequence.Create()
                .Append(LMotion.Create(fromScale, peakScale, phase1Duration)
                    .WithEase(scaleCurve)
                    .BindToLocalScale(transform))
                .Append(LMotion.Create(peakScale, toScale, phase2Duration)
                    .WithEase(scaleCurve)
                    .BindToLocalScale(transform))
                .Run();
        }

        public override void Stop()
        {
            _handle.TryCancel();
            transform.localScale = toScale;
        }

        /// <summary>
        /// Tính toán tọa độ scale dôi ra (peak/bounce scale):
        /// 1. Nếu to > from (tăng): scale quá lên to + scaleExtend rồi mới tụt lại về to.
        /// 2. Nếu to < from (giảm): scale thụt xuống to - scaleExtend rồi mới tăng lại lên to.
        /// </summary>
        private Vector3 CalculatePeakScale()
        {
            float extend = Mathf.Abs(scaleExtend);
            return new Vector3(
                CalculateAxisPeak(fromScale.x, toScale.x, extend),
                CalculateAxisPeak(fromScale.y, toScale.y, extend),
                CalculateAxisPeak(fromScale.z, toScale.z, extend)
            );
        }

        private float CalculateAxisPeak(float from, float to, float extend)
        {
            if (Mathf.Approximately(from, to))
            {
                return to;
            }

            if (to > from)
            {
                return to + extend;
            }
            else
            {
                float peak = to - extend;
                // Nếu đích đến to >= 0 thì không để peak bị âm làm lật ngược UI
                if (to >= 0f && peak < 0f)
                {
                    peak = 0f;
                }
                return peak;
            }
        }
    }
}
