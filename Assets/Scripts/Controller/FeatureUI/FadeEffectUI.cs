using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeEffectUI : EffectBase
    {
        [LabelText("Duration")]
        public float duration = 0.5f;

        [LabelText("From Alpha")]
        [Range(0f, 1f)]
        public float fromAlpha = 0f;

        [LabelText("To Alpha")]
        [Range(0f, 1f)]
        public float toAlpha = 1f;

        [LabelText("Ease")]
        public Ease ease = Ease.OutQuad;

        private CanvasGroup  _canvasGroup;
        private MotionHandle _handle;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
 
        private CanvasGroup CanvasGroup => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        public override void Play()
        {
            _handle.TryCancel();
            CanvasGroup.alpha = fromAlpha;
            _handle = LMotion.Create(fromAlpha, toAlpha, duration)
                             .WithEase(ease)
                             .BindToAlpha(CanvasGroup);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            CanvasGroup.alpha = toAlpha;
        }
    }
}