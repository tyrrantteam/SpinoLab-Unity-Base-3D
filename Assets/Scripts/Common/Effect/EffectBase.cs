using Sirenix.OdinInspector;
using LitMotion;
using UnityEngine;

namespace JinGroup.Common.Effect
{
    public abstract class EffectBase : MonoBehaviour, IEffect
    {
        [SerializeField] private bool canPlayWhenEnable = true;

        [ShowIf("canPlayWhenEnable")]
        [LabelText("Delay Start")]
        [SerializeField] private float delayStart = 0f;

        private MotionHandle _delayHandle;

        protected virtual void OnEnable()
        {
            if (!canPlayWhenEnable) return;

            // Snap về giá trị bắt đầu ngay lập tức trước khi delay
            // → đảm bảo object luôn ở đúng trạng thái khởi đầu khi mở popup
            PrepareInitialState();

            if (delayStart > 0f)
            {
                _delayHandle.TryCancel();
                _delayHandle = LMotion.Create(0f, 1f, delayStart)
                    .WithOnComplete(Play)
                    .RunWithoutBinding();
            }
            else
            {
                Play();
            }
        }

        protected virtual void OnDisable()
        {
            _delayHandle.TryCancel();
            Stop();
        }

        /// <summary>
        /// Gọi tự động khi OnEnable và CanPlayWhenEnable = true.
        /// Override để snap object về trạng thái bắt đầu của hiệu ứng TRƯỚC khi delay.
        /// Đảm bảo khi hết delay, Play() chạy đúng ngay mà không cần set tay trong Unity.
        /// </summary>
        protected virtual void PrepareInitialState() { }

        public abstract void Play();

        public virtual void Stop()
        {
        }
    }
}