using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using JinGroup.Controller.Feature;
using UnityEngine;

namespace JinGroup.Common.Effect
{
    public class EffectController : MonoBehaviour
    {
#if UNITY_EDITOR
        [Button("Add Effect", ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void OnAddEffect(EffectType type)
        {
            EffectBase effect = type switch
            {
                EffectType.ButtonScale => gameObject.AddComponent<ButtonScaleEffectUI>(),
                EffectType.Float      => gameObject.AddComponent<FloatingEffectUI>(),
                EffectType.Move       => gameObject.AddComponent<MovingEffectUI>(),
                EffectType.Shake      => gameObject.AddComponent<RandomShakerUI>(),
                EffectType.Fade         => gameObject.AddComponent<FadeEffectUI>(),
                EffectType.Scale        => gameObject.AddComponent<ScaleEffectUi>(),
                EffectType.ScaleSquashAndStretch => gameObject.AddComponent<ScaleEffectSquashAndStretch>(),
                EffectType.SpawnCircle  => gameObject.AddComponent<SpawnCircleEffectUI>(),
                EffectType.MoveToTarget => gameObject.AddComponent<MoveToTargetEffectUI>(),
                EffectType.Punch        => gameObject.AddComponent<PunchEffectUI>(),
                _                       => null
            };
        }
#endif
    }
}