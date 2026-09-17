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

        public abstract void Play();

        public virtual void Stop()
        {
        }
    }
}