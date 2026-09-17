using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

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

        protected override void PrepareInitialState()
        {
            // Lưu vị trí on-screen hiện tại làm target TRƯỚC khi snap ra ngoài màn hình
            _targetPosition  = RT.anchoredPosition;
            _targetCaptured  = true;

            // Snap ra ngoài màn hình ngay (trước delay) để object không nhìn thấy ở sai vị trí
            RT.anchoredPosition = GetOffScreenPosition(entryDirection);
        }

        public override void Play()
        {
            _handle.TryCancel();

            // Nếu PrepareInitialState chưa chạy (gọi Play() thủ công),
            // capture target và snap off-screen như bình thường
            if (!_targetCaptured)
            {
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
