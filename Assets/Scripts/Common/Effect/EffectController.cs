using Sirenix.OdinInspector;
using JinGroup.Controller.Feature;
using UnityEngine;

namespace JinGroup.Common.Effect
{
    public class EffectController : MonoBehaviour
    {
        private EffectBase[] _effects;

        private void Awake()
        {
            CacheEffects();
            PrepareInitialState();
        }

        private void OnDisable()
        {
            PrepareInitialState();
        }

        public void CacheEffects()
        {
            if (_effects == null || _effects.Length == 0)
            {
                _effects = GetComponents<EffectBase>();
                if (_effects == null || _effects.Length == 0)
                {
                    _effects = GetComponentsInChildren<EffectBase>(true);
                }
            }
        }

        /// <summary>
        /// Reset tất cả effect về trạng thái ban đầu ngay lập tức.
        /// Ưu tiên các effect mở (CanPlayWhenEnable = true), bỏ qua close effect để không bị đè scale=1 / alpha=1 gây nháy.
        /// </summary>
        public void PrepareInitialState()
        {
            CacheEffects();
            if (_effects == null || _effects.Length == 0) return;

            bool hasOpenEffect = false;
            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i] != null && _effects[i].enabled && _effects[i].CanPlayWhenEnable)
                {
                    hasOpenEffect = true;
                    break;
                }
            }

            for (int i = 0; i < _effects.Length; i++)
            {
                var effect = _effects[i];
                if (effect == null || !effect.enabled) continue;

                if (hasOpenEffect)
                {
                    if (effect.CanPlayWhenEnable)
                    {
                        effect.PrepareInitialState();
                    }
                }
                else if (!effect.IsCloseEffect)
                {
                    effect.PrepareInitialState();
                }
            }
        }

        /// <summary>
        /// Alias tiện dụng cho PrepareInitialState
        /// </summary>
        public void ResetAllEffects() => PrepareInitialState();

        public void PlayAll()
        {
            CacheEffects();
            if (_effects == null || _effects.Length == 0) return;

            bool hasOpenEffect = false;
            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i] != null && _effects[i].enabled && _effects[i].CanPlayWhenEnable)
                {
                    hasOpenEffect = true;
                    break;
                }
            }

            for (int i = 0; i < _effects.Length; i++)
            {
                var effect = _effects[i];
                if (effect == null || !effect.enabled) continue;

                if (hasOpenEffect)
                {
                    if (effect.CanPlayWhenEnable)
                    {
                        effect.PrepareInitialState();
                        effect.Play();
                    }
                }
                else if (!effect.IsCloseEffect)
                {
                    effect.PrepareInitialState();
                    effect.Play();
                }
            }
        }

        public void StopAll()
        {
            CacheEffects();
            if (_effects == null) return;

            for (int i = 0; i < _effects.Length; i++)
            {
                if (_effects[i] != null)
                {
                    _effects[i].Stop();
                }
            }
        }

#if UNITY_EDITOR
        [Title("Preview All Effects")]
        [ButtonGroup("Preview All Effects/Buttons")]
        [Button("▶ Play All", ButtonSizes.Large), GUIColor(0.35f, 0.85f, 0.45f)]
        public void PreviewPlayAll()
        {
            EffectBase.EnableEditorMotionScheduler();
            PrepareInitialState();
            PlayAll();
            EffectBase.TriggerEditorRepaint(3f);
        }

        [ButtonGroup("Preview All Effects/Buttons")]
        [Button("⏹ Stop All", ButtonSizes.Large), GUIColor(1f, 0.45f, 0.45f)]
        public void PreviewStopAll()
        {
            StopAll();
            EffectBase.CancelEditorRepaint();
        }

        [ButtonGroup("Preview All Effects/Buttons")]
        [Button("↺ Reset All", ButtonSizes.Large), GUIColor(0.85f, 0.85f, 0.85f)]
        public void PreviewResetAll()
        {
            StopAll();
            PrepareInitialState();
            EffectBase.CancelEditorRepaint();
        }

        [Title("One-Time Effects")]
        [Button("Add Effect", ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void OnAddEffect(EffectType type)
        {
            EffectBase effect = type switch
            {
                EffectType.ButtonScale          => gameObject.AddComponent<ButtonScaleEffectUI>(),
                EffectType.Move                 => gameObject.AddComponent<MovingEffectUI>(),
                EffectType.Shake                => gameObject.AddComponent<RandomShakerUI>(),
                EffectType.Fade                 => gameObject.AddComponent<FadeEffectUI>(),
                EffectType.Scale                => gameObject.AddComponent<ScaleEffectUi>(),
                EffectType.ScaleSquashAndStretch => gameObject.AddComponent<ScaleEffectSquashAndStretch>(),
                EffectType.ScaleBounce          => gameObject.AddComponent<ScaleEffectBounceUI>(),
                EffectType.SpawnCircle          => gameObject.AddComponent<SpawnCircleEffectUI>(),
                EffectType.MoveToTarget         => gameObject.AddComponent<MoveToTargetEffectUI>(),
                EffectType.Punch                => gameObject.AddComponent<PunchEffectUI>(),
                _                               => null
            };
        }

        [Title("Loop Effects")]
        [Button("Add Loop Effect", ButtonSizes.Large), GUIColor(0.2f, 0.8f, 1f)]
        public void OnAddLoopEffect(EffectTypeLoop type)
        {
            EffectBase effect = type switch
            {
                EffectTypeLoop.Float          => gameObject.AddComponent<FloatingEffectUI>(),
                EffectTypeLoop.BreathScale    => gameObject.AddComponent<BreathScaleEffectUI>(),
                EffectTypeLoop.BreathAlpha    => gameObject.AddComponent<BreathAlphaEffectUI>(),
                EffectTypeLoop.Rotate                => gameObject.AddComponent<RotateEffectUI>(),
                EffectTypeLoop.RotatePingPong        => gameObject.AddComponent<RotatePingPongEffectUI>(),
                EffectTypeLoop.ScaleSquashAndStretch => gameObject.AddComponent<ScaleSquashAndStretchLoopEffectUI>(),
                EffectTypeLoop.MirrorShine           => gameObject.AddComponent<UIMirrorShineLoopEffectUI>(),
                _                                    => null
            };
        }
#endif
    }
}
