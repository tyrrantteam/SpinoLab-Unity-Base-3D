using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Controller.Feature
{
    public class MovingEffectUI : EffectBase
    {
        [LabelText("Entry Direction"), EnumToggleButtons]
        public EntryDirection entryDirection = EntryDirection.FromLeft;

        [LabelText("Duration")]
        public float duration = 0.5f;

        private RectTransform _rt;
        private Vector2       _targetPosition;
        private MotionHandle  _handle;
        private bool          _targetCaptured;

        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());

        protected override void Awake()
        {
            _rt = GetComponent<RectTransform>();
            CaptureTargetPosition();
            base.Awake();
        }

        private void CaptureTargetPosition()
        {
            if (!_targetCaptured)
            {
                if (transform.parent is RectTransform parentRT)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
                }

                _targetPosition = RT.anchoredPosition;
                _targetCaptured = true;
            }
        }

        public override void PrepareInitialState()
        {
            CaptureTargetPosition();
            // Snap ra ngoài màn hình ngay lập tức để object không bị nhìn thấy trên màn hình
            RT.anchoredPosition = GetOffScreenPosition(entryDirection);
        }

        public override void Play()
        {
            _handle.TryCancel();
            CaptureTargetPosition();

            RT.anchoredPosition = GetOffScreenPosition(entryDirection);

            _handle = LMotion.Create(RT.anchoredPosition, _targetPosition, duration)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPosition(RT);
        }

        public override void CancelMotion()
        {
            _handle.TryCancel();
        }

        public override void Stop()
        {
            _handle.TryCancel();
            if (_targetCaptured)
            {
                RT.anchoredPosition = _targetPosition;
            }
        }

        private Vector2 GetOffScreenPosition(EntryDirection direction)
        {
            RectTransform parent = RT.parent as RectTransform;
            Vector2 screenSize = parent != null && parent.rect.width > 0f && parent.rect.height > 0f
                ? parent.rect.size
                : new Vector2(Screen.width > 0 ? Screen.width : 1920f, Screen.height > 0 ? Screen.height : 1080f);

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
