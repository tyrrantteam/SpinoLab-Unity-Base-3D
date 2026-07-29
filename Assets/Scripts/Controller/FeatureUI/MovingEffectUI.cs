using System;
using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace JinGroup.Controller.Feature
{
    public class MovingEffectUI : EffectBase
    {
        [LabelText("Entry Direction"), EnumToggleButtons]
        public EntryDirection entryDirection = EntryDirection.FromLeft;

        [LabelText("Duration")]
        public float duration = 0.5f;
    
        private RectTransform _rt;
        private Vector2 _targetPosition;
        private MotionHandle _handle;
        
        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());
        
        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public override void Play()
        {
            _handle.TryCancel();
            _targetPosition = RT.anchoredPosition;
            Vector2 offScreenPosition = GetOffScreenPosition(entryDirection);
            RT.anchoredPosition = offScreenPosition;
            _handle = LMotion.Create(offScreenPosition, _targetPosition, duration)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPosition(RT);
        }

        public override void Stop()
        {
            _handle.TryCancel();
        }

        private Vector2 GetOffScreenPosition(EntryDirection direction)
        {
            Vector2 screenSize = ((RectTransform)_rt.parent).rect.size;
            return direction switch
            {
                EntryDirection.FromLeft   => new Vector2(-screenSize.x, _targetPosition.y),
                EntryDirection.FromRight  => new Vector2(screenSize.x,  _targetPosition.y),
                EntryDirection.FromTop    => new Vector2(_targetPosition.x, screenSize.y),
                EntryDirection.FromBottom => new Vector2(_targetPosition.x, -screenSize.y),
                _                         => _targetPosition
            };
        }
    }
}

public enum EntryDirection
{
    FromLeft,
    FromRight,
    FromTop,
    FromBottom
}
