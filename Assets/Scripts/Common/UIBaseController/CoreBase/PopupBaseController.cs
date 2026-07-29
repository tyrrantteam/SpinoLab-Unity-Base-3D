using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.UIBaseController
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PopupBaseController : MonoBehaviour
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button closePanelBtn;

        [Header("Close Animation")]

        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField] private Ease closeEase = Ease.InQuad;

        [Header("Close Input")]
        [SerializeField] private float closeBlockDuration = 0.8f;

        private Transform scaleTarget;
        private CanvasGroup canvasGroup;
        private MotionHandle _closeHandle;
        private Vector3 _openScale;
        private float _openAlpha;
        private bool _isClosing;
        private bool _canClose;

        protected virtual void Awake()
        {
            if (scaleTarget == null)
                scaleTarget = transform;

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            _openScale = scaleTarget.localScale;
            _openAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            ListenerButton();
            InitData();
        }

        protected virtual void OnEnable()
        {
            ResetOpenVisual();
            BeginCloseBlock();
        }

        protected virtual void OnDisable()
        {
            CancelInvoke(nameof(EnableClose));
            _canClose = false;
            _closeHandle.TryCancel();
            _isClosing = false;
        }

        protected virtual void ListenerButton()
        {
            if (closeBtn != null)
                closeBtn.onClick.AddListener(OnClosePopup);

            if (closePanelBtn != null)
                closePanelBtn.onClick.AddListener(OnClosePopup);
        }

        protected virtual void InitData()
        {
        }

        private void BeginCloseBlock()
        {
            _canClose = false;
            CancelInvoke(nameof(EnableClose));
            Invoke(nameof(EnableClose), closeBlockDuration);
        }

        private void EnableClose()
        {
            _canClose = true;
        }

        /// <summary>Restores scale and canvas group alpha for the next open.</summary>
        protected virtual void ResetOpenVisual()
        {
            _closeHandle.TryCancel();
            _isClosing = false;
            scaleTarget.localScale = _openScale;

            if (canvasGroup != null)
                canvasGroup.alpha = _openAlpha;
        }

        /// <summary>Scales <see cref="scaleTarget"/> to zero and fades <see cref="canvasGroup"/> to zero.</summary>
        protected virtual void AnimateCloseToZero(Action onComplete)
        {
            _closeHandle.TryCancel();

            Vector3 fromScale = scaleTarget.localScale;
            float fromAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            _closeHandle = LMotion.Create(0f, 1f, closeDuration)
                                  .WithEase(closeEase)
                                  .WithOnComplete(() => onComplete?.Invoke())
                                  .Bind(t =>
                                  {
                                      scaleTarget.localScale = Vector3.LerpUnclamped(fromScale, Vector3.zero, t);

                                      if (canvasGroup != null)
                                          canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, 0f, t);
                                  });

        }

        protected virtual void OnClosePopup()
        {
            if (!_canClose || _isClosing)
                return;

            _isClosing = true;

            AnimateCloseToZero(() =>
            {
                PopupManager.Instance.CloseCurrentPopup();
                gameObject.SetActive(false);
                _isClosing = false;
            });
        }
    }
}
