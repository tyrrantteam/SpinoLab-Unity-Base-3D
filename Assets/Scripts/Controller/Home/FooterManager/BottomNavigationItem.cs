using Base.Core.Sound;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// One bottom navigation tab: input, visuals, content panel, and optional notification badge.
/// </summary>
public class BottomNavigationItem : MonoBehaviour
{
    [SerializeField, Tooltip("Unique id used by BottomNavigationController.SelectTab(string).")]
    private string tabId;

    [FormerlySerializedAs("scene")]
    [SerializeField]
    private TabType tabType = TabType.Home;

    [SerializeField] private Toggle toggle;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text label;

    [SerializeField, Tooltip("Panel shown when this tab is selected.")]
    private GameObject contentPanel;

    [SerializeField, Tooltip("Optional red-dot or count badge.")]
    private GameObject notificationBadge;

    [SerializeField] private BottomNavigationVisualSettings visualSettings = BottomNavigationVisualSettings.Default;

    [Header("Move Active tab")]
    [SerializeField] private RectTransform endPoint;
    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform activeButton;
    [SerializeField] private float moveDuration = 0.3f;

    [Header("Icon motion")]
    [SerializeField] private RectTransform icon;
    [SerializeField] private RectTransform iconStartPoint;
    [SerializeField] private RectTransform iconEndPoint;
    [SerializeField] private float imgScale = 1.5f;

    public UnityEvent<TabType> OnClickTabButton;

    public string TabId => tabId;
    public TabType TabType => tabType;
    public GameObject ContentPanel => contentPanel;

    private BottomNavigationController _owner;
    private Image _iconImageCached;
    private bool _suppressToggleCallback;

    private void Reset()
    {
        tabId = tabType.ToString();
        if (toggle == null)
            toggle = GetComponent<Toggle>();
        if (icon != null && iconImage == null)
            iconImage = icon.GetComponent<Image>();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(tabId))
            tabId = tabType.ToString();

        if (toggle == null)
            toggle = GetComponent<Toggle>();

        if (icon != null && iconImage == null)
            iconImage = icon.GetComponent<Image>();
    }

    /// <summary>
    /// Called once by <see cref="BottomNavigationController"/> to wire selection.
    /// </summary>
    public void Initialize(BottomNavigationController owner)
    {
        _owner = owner;
        CacheIconImage();

        if (toggle == null)
        {
            Debug.LogError($"[{nameof(BottomNavigationItem)}] Missing Toggle on '{name}'.", this);
            return;
        }

        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    public void Uninitialize()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);

        KillTweens();
        _owner = null;
    }

    private void OnDestroy()
    {
        Uninitialize();
    }

    /// <summary>
    /// Sets toggle state without notifying the controller.
    /// </summary>
    public void SetToggleOnSilently(bool isOn)
    {
        if (toggle == null)
            return;

        _suppressToggleCallback = true;
        toggle.isOn = isOn;
        _suppressToggleCallback = false;
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (_suppressToggleCallback || _owner == null)
            return;

        if (!isOn)
            return;

        _owner.NotifyTabSelected(this);
    }

    public void PlayButtonSound()
    {
        if (SoundManager.Instance == null)
            return;

        SoundManager.Instance.PlaySound(SoundType.button_click);
    }

    /// <summary>
    /// Applies selected or unselected motion and colors.
    /// </summary>
    public void ApplyVisualState(bool selected)
    {
        CacheIconImage();
        KillTweens();

        if (activeButton != null && startPoint != null && endPoint != null)
        {
            var target = selected ? endPoint.anchoredPosition : startPoint.anchoredPosition;
            activeButton.DOAnchorPos(target, moveDuration);
        }

        if (icon != null && iconStartPoint != null && iconEndPoint != null)
        {
            var iconTarget = selected ? iconEndPoint.anchoredPosition : iconStartPoint.anchoredPosition;
            icon.DOScale(selected ? imgScale : 1f, moveDuration);
            icon.DOAnchorPos(iconTarget, moveDuration);
        }

        var iconColor = selected ? visualSettings.selectedIconColor : visualSettings.unselectedIconColor;
        if (_iconImageCached != null)
            _iconImageCached.DOColor(iconColor, moveDuration);

        if (label != null)
        {
            var labelColor = selected ? visualSettings.selectedLabelColor : visualSettings.unselectedLabelColor;
            label.DOColor(labelColor, moveDuration);
        }
    }

    public void SetNotificationVisible(bool visible)
    {
        if (notificationBadge != null)
            notificationBadge.SetActive(visible);
    }

    public void InvokeClickEvent()
    {
        OnClickTabButton?.Invoke(tabType);
    }

    private void CacheIconImage()
    {
        if (_iconImageCached != null)
            return;

        if (iconImage != null)
            _iconImageCached = iconImage;
        else if (icon != null)
            _iconImageCached = icon.GetComponent<Image>();
    }

    private void KillTweens()
    {
        if (activeButton != null)
            activeButton.DOKill();

        if (icon != null)
            icon.DOKill();

        if (_iconImageCached != null)
            _iconImageCached.DOKill();

        if (label != null)
            label.DOKill();
    }

#if UNITY_EDITOR
    [Button]
    private void PreviewSelected() => ApplyVisualState(true);

    [Button]
    private void PreviewUnselected() => ApplyVisualState(false);
#endif
}
