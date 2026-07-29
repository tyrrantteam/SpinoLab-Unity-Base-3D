using System;
using Base.Core.Sound;
using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinGroup.Controller.Feature
{
    [RequireComponent(typeof(Button))]
    public class ButtonScaleEffectUI : EffectBase, IPointerClickHandler
    {
        [LabelText("Scale Multiplier")]
        public float scaleMultiplier = 1.2f;

        [LabelText("Duration")]
        public float duration = 0.1f;

        private MotionHandle _handle;

        public override void Play()
        {
            _handle.TryCancel();
           SoundManager.Instance.PlaySound(SoundType.ClickButton);
            var strength = new Vector3(scaleMultiplier - 1f, scaleMultiplier - 1f, scaleMultiplier - 1f);
            _handle = LMotion.Punch.Create(Vector3.one, strength, duration * 2f)
                .BindToLocalScale(transform);
        }
        
        public override void Stop()
        {
            _handle.TryCancel();
            transform.localScale = Vector3.one;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Play();
        }
    }
}
