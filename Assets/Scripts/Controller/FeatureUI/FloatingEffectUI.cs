using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JinGroup.Controller.Feature
{
    public class FloatingEffectUI : EffectBase
    {
        [LabelText("Float Strength")]
        public float floatStrength = 10f;

        [LabelText("Float Speed")]
        public float floatSpeed = 1f;

        private RectTransform _rt;
        private Vector2       _initialPos;
        private MotionHandle  _handle;
        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());
        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public override void Play()
        {
            _handle.TryCancel();
            _initialPos = RT.anchoredPosition;
            float halfPeriod = Mathf.PI / floatSpeed;
            _handle = LMotion.Create(_initialPos.y - floatStrength, _initialPos.y + floatStrength, halfPeriod)
                .WithEase(Ease.InOutSine)
                .WithLoops(-1, LoopType.Yoyo)
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
