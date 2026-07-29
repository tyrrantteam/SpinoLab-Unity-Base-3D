using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Common.Effect
{
    public abstract class EffectBase : MonoBehaviour, IEffect
    {
        [SerializeField] private bool canplayWhenEnable = true;

        protected virtual void OnEnable()
        {
            if (!canplayWhenEnable) return;
            Play();
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        public abstract void Play();

        public virtual void Stop()
        {
        }
    }
}