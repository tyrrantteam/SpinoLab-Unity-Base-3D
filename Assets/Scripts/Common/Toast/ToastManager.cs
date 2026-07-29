/*  SETUP INSTRUCTIONS
 *  ──────────────────────────────────────────────────────────────────────────
 *  1. Add ToastManager to a persistent scene GameObject.
 *  2. Create a child GameObject under it (the toast panel):
 *       • Add a CanvasGroup component to it.
 *       • Add an Image with Image Type = Filled (the fill indicator).
 *       • Add a TextMeshPro – Text (UI) child named "Label".
 *       • Add the ToastItem component and wire up all four references.
 *  3. Drag that child into the ToastManager.toastItem field in the Inspector.
 *     The manager will SetActive(false) it on Awake automatically.
 *  ──────────────────────────────────────────────────────────────────────────
 *
 *  USAGE
 *  ──────────────────────────────────────────────────────────────────────────
 *  // First call — plays fill animation, fires callback, then shows text
 *  ToastManager.Instance.Show("Level Up!", onFillComplete: () => Debug.Log("filled!"));
 *
 *  // While already visible — only the label changes, no fill replay
 *  ToastManager.Instance.Show("New message");
 *  ──────────────────────────────────────────────────────────────────────────
 */

using System;
using System.Collections;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class ToastManager : SingletonMono<ToastManager>
{
    private enum ToastState { Idle, FillingIn, ShowingText, FadingOut }

    // -------------------------------------------------------------------------
    // Inspector
    // -------------------------------------------------------------------------

    [Title("References")]
    [SerializeField] private ToastItem toastItem;

    [Title("Timing")]
    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private float fillDuration    = 0.8f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float slideOffset     = 60f;

    [Title("Easing")]
    [SerializeField] private Ease fillEase  = Ease.OutCubic;
    [SerializeField] private Ease easeOut   = Ease.InCubic;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private ToastItem  _item => toastItem;
    private ToastState _state            = ToastState.Idle;
    private float      _remainingDuration;
    private string     _pendingText;
    private float      _restY;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <param name="text">Message to display after the fill completes.</param>
    /// <param name="duration">How long the text stays visible. Uses defaultDuration when negative.</param>
    /// <param name="onFillComplete">Fired once the fill animation finishes (only on the first call while Idle).</param>
    [Button]
    public void Show(string text, float duration = -1f, Action onFillComplete = null)
    {
        if (duration < 0f) duration = defaultDuration;

        switch (_state)
        {
            case ToastState.Idle:
                StartCoroutine(RunToast(text, duration, onFillComplete));
                break;

            case ToastState.FillingIn:
                // Fill is still running — queue the text for when it finishes
                _pendingText        = text;
                _remainingDuration  = duration;
                break;

            case ToastState.ShowingText:
            case ToastState.FadingOut:
                // Already visible — swap text and restart the hold timer
                _item.Label.text   = text;
                _remainingDuration = duration;
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Coroutine
    // -------------------------------------------------------------------------

    private IEnumerator RunToast(string text, float duration, Action onFillComplete)
    {
        _state             = ToastState.FillingIn;
        _pendingText       = text;
        _remainingDuration = duration;

        // --- Prepare item ---
        _item.FillImage.fillAmount        = 0f;
        _item.CanvasGroup.alpha            = 1f;
        _item.Label.gameObject.SetActive(false);
        _item.gameObject.SetActive(true);
        _restY = _item.RectTransform.anchoredPosition.y;

        // --- Fill animation ---
        MotionHandle fillHandle = LMotion.Create(0f, 1f, fillDuration)
            .WithEase(fillEase)
            .Bind(v => _item.FillImage.fillAmount = v);

        yield return new WaitForSeconds(fillDuration);
        if (!fillHandle.TryComplete()) fillHandle.TryCancel();
        _item.FillImage.fillAmount = 1f;

        // --- Show text after fill ---
        onFillComplete?.Invoke();
        _item.Label.text = _pendingText;
        _item.Label.gameObject.SetActive(true);

        _state = ToastState.ShowingText;

        // --- Hold (interruptible — _remainingDuration can be changed by Show()) ---
        while (_remainingDuration > 0f)
        {
            _remainingDuration -= Time.deltaTime;
            yield return null;
        }

        _state = ToastState.FadingOut;

        // --- Fade + slide out ---
        MotionHandle fadeHandle = LMotion.Create(1f, 0f, fadeOutDuration)
            .WithEase(easeOut)
            .BindToAlpha(_item.CanvasGroup);

        LMotion.Create(_restY, _restY - slideOffset, fadeOutDuration)
            .WithEase(easeOut)
            .BindToAnchoredPositionY(_item.RectTransform);

        // Allow interruption during fade: if _remainingDuration was reset by
        // a new Show() call, abort the fade and stay visible.
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeOutDuration && _state == ToastState.FadingOut)
        {
            fadeElapsed += Time.deltaTime;
            yield return null;
        }

        if (!fadeHandle.TryComplete()) fadeHandle.TryCancel();

        if (_state == ToastState.FadingOut)
        {
            // Normal completion — hide the item
            _item.gameObject.SetActive(false);
            _item.CanvasGroup.alpha = 1f;
            _item.RectTransform.anchoredPosition = new Vector2(
                _item.RectTransform.anchoredPosition.x, _restY);
            _state = ToastState.Idle;
        }
        else
        {
            // Interrupted by a new Show() call — restore alpha and stay visible
            _item.CanvasGroup.alpha = 1f;
            _item.RectTransform.anchoredPosition = new Vector2(
                _item.RectTransform.anchoredPosition.x, _restY);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (toastItem == null)
            Debug.LogError("[ToastManager] toastItem is not assigned! Drag the child ToastItem into the field.");
        else
            toastItem.gameObject.SetActive(false);
    }
}
