using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class FloatingEffectUI : EffectBaseLoop
    {
        [LabelText("Float Strength")]
        public float floatStrength = 10f;

        [LabelText("Ease")]
        public Ease ease = Ease.InOutSine;

        private RectTransform _rt;
        private Vector2       _initialPos;
        private MotionHandle  _handle;

        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());
        private bool          _isCaptured;

        protected override void Awake()
        {
            _rt = GetComponent<RectTransform>();
            CaptureInitialState();
            base.Awake();
        }

        private void CaptureInitialState()
        {
            if (!_isCaptured)
            {
                _initialPos = RT.anchoredPosition;
                _isCaptured = true;
            }
        }

        public override void CancelMotion()
        {
            _handle.TryCancel();
        }

        public override void PrepareInitialState()
        {
            CaptureInitialState();
            if (RT != null)
                RT.anchoredPosition = _initialPos;
        }

        public override void Play()
        {
            _handle.TryCancel();
            _handle = LMotion.Create(_initialPos.y - floatStrength, _initialPos.y + floatStrength, duration)
                .WithEase(ease)
                .WithLoops(LoopCount, LoopType.Yoyo)
                .BindToAnchoredPositionY(RT);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            if (RT != null)
                RT.anchoredPosition = _initialPos;
        }
    }
}
