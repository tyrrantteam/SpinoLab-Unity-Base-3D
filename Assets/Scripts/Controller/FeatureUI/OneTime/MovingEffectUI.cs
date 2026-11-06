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

        // Flag: PrepareInitialState đã lưu _targetPosition rồi — Play() không capture lại
        private bool _targetCaptured;

        private RectTransform RT => _rt != null ? _rt : (_rt = GetComponent<RectTransform>());

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
        }

        public override void PrepareInitialState()
        {
            if (!_targetCaptured)
            {
                if (transform.parent is RectTransform parentRT)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
                }

                _targetPosition  = RT.anchoredPosition;
                _targetCaptured  = true;
            }

            // Snap ra ngoài màn hình ngay lập tức để object không bị nhìn thấy trên màn hình
            RT.anchoredPosition = GetOffScreenPosition(entryDirection);
        }

        public override void Play()
        {
            _handle.TryCancel();

            // Nếu PrepareInitialState chưa chạy (gọi Play() thủ công),
            // capture target và snap off-screen như bình thường
            if (!_targetCaptured)
            {
                if (transform.parent is RectTransform parentRT)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRT);
                }

                _targetPosition = RT.anchoredPosition;
                RT.anchoredPosition = GetOffScreenPosition(entryDirection);
            }
            _targetCaptured = false;

            _handle = LMotion.Create(RT.anchoredPosition, _targetPosition, duration)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPosition(RT);
        }

        public override void Stop()
        {
            _handle.TryCancel();
            RT.anchoredPosition = _targetPosition;
            _targetCaptured = false;
        }

        private Vector2 GetOffScreenPosition(EntryDirection direction)
        {
            RectTransform parent = _rt.parent as RectTransform;
            Vector2 screenSize = parent != null ? parent.rect.size : new Vector2(Screen.width, Screen.height);
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
