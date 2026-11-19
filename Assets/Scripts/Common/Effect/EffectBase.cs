using Sirenix.OdinInspector;
using LitMotion;
using UnityEngine;
#if UNITY_EDITOR
using System;
using LitMotion.Editor;
using UnityEditor;
#endif

namespace JinGroup.Common.Effect
{
    public abstract class EffectBase : MonoBehaviour, IEffect
    {
        [UnityEngine.Serialization.FormerlySerializedAs("canplayWhenEnable")]
        [SerializeField] private bool canPlayWhenEnable = true;

        /// <summary>
        /// Cho biết hiệu ứng này có phải hiệu ứng đóng (scale down hoặc fade out) hay không.
        /// Hiệu ứng đóng không bao giờ tự động chạy khi GameObject được enable.
        /// </summary>
        public virtual bool IsCloseEffect => false;

        public bool CanPlayWhenEnable
        {
            get => canPlayWhenEnable && !IsCloseEffect;
            set => canPlayWhenEnable = value;
        }

        [ShowIf("CanPlayWhenEnable")] [LabelText("Delay Start")] [SerializeField]
        private float delayStart = 0f;

        private MotionHandle _delayHandle;

        protected virtual void Awake()
        {
            if (CanPlayWhenEnable)
            {
                PrepareInitialState();
            }
        }

        protected virtual void OnEnable()
        {
            if (!CanPlayWhenEnable) return;

            // Snap về giá trị bắt đầu ngay lập tức khi enable (trước khi render frame đầu tiên và trước delay)
            // → đảm bảo object luôn ở đúng trạng thái khởi đầu ngay từ frame đầu tiên khi mở popup
            PrepareInitialState();

            if (delayStart > 0f)
            {
                _delayHandle.TryCancel();
                _delayHandle = LMotion.Create(0f, 1f, delayStart)
                                      .WithOnComplete(Play)
                                      .RunWithoutBinding();
            }
            else
            {
                Play();
            }
        }

        protected virtual void OnDisable()
        {
            _delayHandle.TryCancel();
            CancelMotion();

            // Reset về trạng thái ban đầu ngay khi disable để lần mở tiếp theo không bị nháy hình ở frame đầu.
            // Hiệu ứng đóng (IsCloseEffect) không reset về fromScale=1 / fromAlpha=1 để tránh đè lên hiệu ứng mở.
            if (!IsCloseEffect)
            {
                PrepareInitialState();
            }
        }

        /// <summary>
        /// Hủy tween đang chạy mà không ép giá trị về toScale/toAlpha như Stop().
        /// Dùng khi disable hoặc reset để tránh làm nhảy trạng thái visual của object.
        /// </summary>
        public virtual void CancelMotion()
        {
            Stop();
        }

        /// <summary>
        /// Gọi tự động khi Awake, OnEnable, OnDisable (hoặc gọi thủ công trước khi mở).
        /// Override để snap object về trạng thái bắt đầu của hiệu ứng.
        /// Đảm bảo không bị nháy hình ở trạng thái mặc định (alpha=1, scale=1) trước khi diễn animation.
        /// </summary>
        public virtual void PrepareInitialState()
        {
        }
        

        public abstract void Play();

        public virtual void Stop()
        {
        }

#if UNITY_EDITOR
        private static bool _isRepaintHooked;
        private static double _repaintUntilTime;

        public static void EnableEditorMotionScheduler()
        {
            if (!Application.isPlaying)
            {
                MotionScheduler.DefaultScheduler = EditorMotionScheduler.Update;
            }
        }

        public static void TriggerEditorRepaint(float activeDuration = 2f)
        {
            if (Application.isPlaying) return;

            _repaintUntilTime = Math.Max(_repaintUntilTime, EditorApplication.timeSinceStartup + activeDuration);

            if (!_isRepaintHooked)
            {
                _isRepaintHooked = true;
                EditorApplication.update += OnEditorRepaintTick;
            }
        }

        public static void CancelEditorRepaint()
        {
            if (_isRepaintHooked)
            {
                _isRepaintHooked = false;
                EditorApplication.update -= OnEditorRepaintTick;
            }
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private static void OnEditorRepaintTick()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();

            if (EditorApplication.timeSinceStartup > _repaintUntilTime)
            {
                CancelEditorRepaint();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSchedulerOnPlay()
        {
            MotionScheduler.DefaultScheduler = MotionScheduler.Update;
        }

        [Title("Preview Effect")]
        [ButtonGroup("Preview Effect/Buttons")]
        [Button("▶ Play", ButtonSizes.Medium), GUIColor(0.35f, 0.85f, 0.45f)]
        public void PreviewPlay()
        {
            EnableEditorMotionScheduler();
            PrepareInitialState();
            Play();
            TriggerEditorRepaint(GetPreviewDuration());
        }

        [ButtonGroup("Preview Effect/Buttons")]
        [Button("⏹ Stop", ButtonSizes.Medium), GUIColor(1f, 0.45f, 0.45f)]
        public void PreviewStop()
        {
            Stop();
            CancelEditorRepaint();
        }

        [ButtonGroup("Preview Effect/Buttons")]
        [Button("↺ Reset", ButtonSizes.Medium), GUIColor(0.85f, 0.85f, 0.85f)]
        public void PreviewReset()
        {
            Stop();
            PrepareInitialState();
            CancelEditorRepaint();
        }

        protected virtual float GetPreviewDuration() => 2f;
#endif
    }
}
