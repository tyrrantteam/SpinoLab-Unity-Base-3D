using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    /// <summary>
    /// Alpha đậm nhạt liên tục theo kiểu "thở" (Yoyo loop) qua CanvasGroup.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class BreathAlphaEffectUI : EffectBaseLoop
    {
        [Title("Breath Alpha Config")]
        [LabelText("Max Alpha"), Range(0f, 1f)]
        public float maxAlpha = 1f;

        [LabelText("Min Alpha"), Range(0f, 1f)]
        public float minAlpha = 0f;

        [LabelText("Ease")]
        public Ease ease = Ease.InOutSine;

        private CanvasGroup  _canvasGroup;
        private MotionHandle _handle;
        private float        _initialAlpha;

        private CanvasGroup CanvasGroup => _canvasGroup != null
            ? _canvasGroup
            : (_canvasGroup = GetComponent<CanvasGroup>());

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Play()
        {
            _handle.TryCancel();
            _initialAlpha = CanvasGroup.alpha;
            _handle = LMotion.Create(minAlpha, maxAlpha, duration)
                .WithEase(ease)
                .WithLoops(LoopCount, LoopType.Yoyo)
                .BindToAlpha(CanvasGroup);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            CanvasGroup.alpha = _initialAlpha;
        }
    }
}
