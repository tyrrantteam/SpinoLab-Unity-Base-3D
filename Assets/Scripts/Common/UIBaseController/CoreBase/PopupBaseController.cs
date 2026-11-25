using System;
using JinGroup.Common.Effect;
using JinGroup.Controller.Feature;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PopupBaseController : MonoBehaviour
{
    [SerializeField] private Button        closeBtn;
    [SerializeField] private Button        closePanelBtn;
    [SerializeField] private ScaleEffectUi scaleEffectUi;

    [Header("Close Animation")] [SerializeField]
    private float closeDuration = 0.25f;

    [SerializeField] private Ease closeEase = Ease.InQuad;

    [Header("Close Input")] [SerializeField]
    private float closeBlockDuration = 0.8f;

    private Transform    scaleTarget;
    private CanvasGroup  canvasGroup;
    private MotionHandle _closeHandle;
    private Vector3      _openScale;
    private float        _openAlpha;
    protected bool       _isClosing;
    protected bool       _canClose;

    protected virtual void Awake()
    {
        if (scaleTarget == null)
            scaleTarget = transform;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        _openScale = scaleTarget.localScale;
        _openAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        if (scaleEffectUi != null)
        {
            scaleEffectUi.CanPlayWhenEnable = false;
        }

        InitData();
        ListenerButton();

        ResetToInitialState();
    }

    protected virtual void OnEnable()
    {
        transform.SetAsLastSibling();
        ResetOpenVisual();
        BeginCloseBlock();
    }

    protected virtual void OnDisable()
    {
        CancelInvoke(nameof(EnableClose));
        _canClose = false;
        _closeHandle.TryCancel();
        _isClosing = false;

        ResetToInitialState();
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

    /// <summary>
    /// Reset popup về trạng thái ban đầu khi tắt popup (khi close xong hoặc OnDisable).
    /// Đảm bảo khi mở lại, popup đã ở sẵn trạng thái bắt đầu của hiệu ứng mở (scale = fromScale, alpha = fromAlpha),
    /// không bị render ở trạng thái hiện rồi mới nháy về 0 để diễn anim.
    /// </summary>
    public virtual void ResetToInitialState()
    {
        _closeHandle.TryCancel();
        _isClosing = false;

        if (scaleEffectUi != null)
        {
            scaleEffectUi.CanPlayWhenEnable = false;
        }

        // 1. Reset root scale và canvasGroup về giá trị mặc định lúc đầu
        if (scaleTarget != null)
        {
            scaleTarget.localScale = _openScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = _openAlpha;
        }

        // 2. Đưa tất cả các hiệu ứng mở (EffectBase có CanPlayWhenEnable = true) về trạng thái khởi đầu (PrepareInitialState)
        var effects = GetComponentsInChildren<EffectBase>(true);
        for (int i = 0; i < effects.Length; i++)
        {
            var effect = effects[i];
            if (effect == null || effect == scaleEffectUi || effect.IsCloseEffect) continue;

            if (effect.CanPlayWhenEnable)
            {
                effect.PrepareInitialState();
            }
        }

        // 3. Reset các EffectController trong popup về trạng thái ban đầu
        var effectControllers = GetComponentsInChildren<EffectController>(true);
        for (int i = 0; i < effectControllers.Length; i++)
        {
            if (effectControllers[i] != null)
            {
                effectControllers[i].PrepareInitialState();
            }
        }
    }

    /// <summary>
    /// Chuẩn bị các trạng thái khi mở popup.
    /// Lưu ý: Không set đè scale=1 hay alpha=1 ở đây nếu có hiệu ứng mở tự quản lý,
    /// vì trạng thái đã được ResetToInitialState() thiết lập sẵn ngay khi tắt popup.
    /// </summary>
    protected virtual void ResetOpenVisual()
    {
        _closeHandle.TryCancel();
        _isClosing = false;

        if (scaleEffectUi != null)
        {
            scaleEffectUi.CanPlayWhenEnable = false;
        }

        // Kiểm tra xem popup có hiệu ứng mở nào đang hoạt động không
        var effects = GetComponentsInChildren<EffectBase>(true);
        bool hasOpenEffect = false;
        for (int i = 0; i < effects.Length; i++)
        {
            if (effects[i] != null && effects[i] != scaleEffectUi && !effects[i].IsCloseEffect && effects[i].CanPlayWhenEnable)
            {
                hasOpenEffect = true;
                break;
            }
        }

        // Nếu popup hoàn toàn không có hiệu ứng mở tự động nào, đảm bảo scale và alpha được hiển thị đầy đủ
        if (!hasOpenEffect)
        {
            if (scaleTarget != null)
                scaleTarget.localScale = _openScale;

            if (canvasGroup != null)
                canvasGroup.alpha = _openAlpha;
        }
    }

    /// <summary>Scales <see cref="scaleTarget"/> to zero and fades <see cref="canvasGroup"/> to zero.</summary>
    protected virtual void AnimateCloseToZero(Action onComplete)
    {
        _closeHandle.TryCancel();

        Vector3 fromScale = scaleTarget.localScale;
        float fromAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        if (scaleEffectUi != null)
            scaleEffectUi.Play();

        _closeHandle = LMotion.Create(0f, 1f, closeDuration)
                              .WithEase(closeEase)
                              .WithOnComplete(() => onComplete?.Invoke())
                              .Bind(t =>
                              {
                                  if (canvasGroup != null)
                                      canvasGroup.alpha = Mathf.LerpUnclamped(fromAlpha, 0f, t);
                              });
    }

    private Action _onClosedCallback;

    public void SetOnClosedCallback(Action callback)
    {
        _onClosedCallback = callback;
    }

    protected virtual void OnClosePopup()
    {
        if (!_canClose || _isClosing)
            return;

        _isClosing = true;

        AnimateCloseToZero(() =>
        {
            gameObject.SetActive(false);
            if (PopupManager.Instance != null)
                PopupManager.Instance.ClosePopupKeepOthers();

            _isClosing = false;
            ResetToInitialState();
            _onClosedCallback?.Invoke();
            _onClosedCallback = null;
            OnClosed();
        });
    }

    protected virtual void OnClosed()
    {
    }

    public virtual void ClosePopup()
    {
        OnClosePopup();
    }
}
