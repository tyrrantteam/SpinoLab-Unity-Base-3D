using System;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.ResourcesHeader;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartResourcesHeader : BaseResourcesHeader
{
    [SerializeField] private TMP_Text   cooldownTxt;
    [SerializeField] private TMP_Text   heartTxt;
    [SerializeField] private GameObject heartImg;
    [SerializeField] private GameObject heartFree;
   
    private const float TickInterval    = 1f; // cooldown text only shows whole seconds
    private const int   DefaultMaxHeart = 5;
    private const long  ShownFull       = -1;
    private const long  ShownFree       = -2;
    private const long  ShownNone       = long.MinValue;

    private DataHeartController _datagame;
    private int                 _maxHeart = DefaultMaxHeart;

    // Cached UI state to avoid redundant text/SetActive updates
    private long _shownCooldownSecs = ShownNone;
    private int  _shownHeart        = int.MinValue;

    // Polling state: only tick while a countdown is running or infinite hearts may expire
    private bool  _needsTick;
    private bool  _isInfinite;
    private float _nextTickTime;

    private Action<Component, object> _onUpdateHeart;

    public static HeartResourcesHeader Instance { get; private set; }

    /// <summary>
    /// Lấy chuỗi cooldown toàn cục từ PlayerResourceData.
    /// Hoạt động đúng ở mọi nơi (Home, Shop, Gameplay, Popups) ngay cả khi instance này bị deactivate.
    /// </summary>
    public static string GetCooldownStringGlobal()
    {
        var data = DataAccountPlayer.PlayerResourceData;
        return data != null ? data.GetHeartCooldownString() : "Full";
    }

    public string CurrentCooldownString => GetCooldownStringGlobal();

    protected override void Awake()
    {
        _onUpdateHeart = (sender, param) => UpdateValue();
        
        base.Awake(); // calls UpdateValue(), which is null-safe
    }

    protected override void OnClickClaimMore()
    {
        base.OnClickClaimMore();
        PopupManager.Instance.ShowPopup<PopupGetMoreHeartController>();
    }

    private void OnEnable()
    {
        Instance = this;

        if (EventDispatcher.Instance != null)
        {
            EventDispatcher.Instance.RegisterListener(EventID.UpdateHeart, _onUpdateHeart);
        }

        ResetShownState();

        var data = DataAccountPlayer.PlayerResourceData;
        if (data != null)
        {
            data.UpdateHeartRecovery();
        }

        UpdateValue();
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;

        if (EventDispatcher.Instance != null)
        {
            EventDispatcher.Instance.RemoveListener(EventID.UpdateHeart, _onUpdateHeart);
        }
    }

    private void Update()
    {
        if (!_needsTick || Time.unscaledTime < _nextTickTime)
            return;

        _nextTickTime = Time.unscaledTime + TickInterval;

        var data = DataAccountPlayer.PlayerResourceData;
        if (data == null)
            return;

        if (_isInfinite)
        {
            // No event fires when infinite hearts expire, so detect it here
            if (!data.IsInfiniteHeartActive())
                UpdateValue();
            return;
        }

        // GetRemainingRecoverySeconds() runs UpdateHeartRecovery() internally.
        // If a heart was recovered it posts UpdateHeart, which already refreshed the whole UI.
        long secs = data.GetRemainingRecoverySeconds();
        if (data.heart != _shownHeart)
        {
            UpdateValue();
            return;
        }

        SetCooldownSeconds(secs);
    }

    protected override void UpdateValue()
    {
        var data = DataAccountPlayer.PlayerResourceData;
        if (data == null)
            return;

        RefreshConfig();

        _isInfinite = data.IsInfiniteHeartActive();
        int heart = data.heart;
        bool isFull = heart >= _maxHeart;

        SetActiveIfChanged(claimMore != null ? claimMore.gameObject : null, !_isInfinite && !isFull);
        SetActiveIfChanged(heartImg, !_isInfinite);
        SetActiveIfChanged(heartFree, _isInfinite);
        if (heartTxt != null && heartTxt != valueTxt)
            SetActiveIfChanged(heartTxt.gameObject, !_isInfinite);

        if (_isInfinite)
        {
            SetCooldownState(ShownFree, "Free");
            _needsTick = true; // watch for expiry
            return;
        }

        valueInformation = heart;
        if (heart != _shownHeart)
        {
            _shownHeart = heart;
            string heartStr = heart.ToString();
            if (heartTxt != null) heartTxt.text = heartStr;
            if (valueTxt != null) valueTxt.text = heartStr;
        }

        if (isFull)
        {
            SetCooldownState(ShownFull, "Full");
            _needsTick = false; // nothing changes until an UpdateHeart event
            return;
        }

        SetCooldownSeconds(data.GetRemainingRecoverySeconds());
        _needsTick    = true;
        _nextTickTime = Time.unscaledTime + TickInterval;
    }

    private void RefreshConfig()
    {
        if (_datagame == null && LoadResourceController.Instance != null)
        {
            _datagame = LoadResourceController.Instance.DataHeartController();
        }

        _maxHeart = _datagame != null ? _datagame.Limit : DefaultMaxHeart;
    }

    private void ResetShownState()
    {
        _shownCooldownSecs = ShownNone;
        _shownHeart        = int.MinValue;
        _nextTickTime      = 0f;
    }

    private void SetCooldownSeconds(long secs)
    {
        if (cooldownTxt == null || secs == _shownCooldownSecs)
            return;

        _shownCooldownSecs = secs;
        cooldownTxt.text   = $"{secs / 60:D2}:{secs % 60:D2}";
    }

    private void SetCooldownState(long state, string text)
    {
        if (cooldownTxt == null || _shownCooldownSecs == state)
            return;

        _shownCooldownSecs = state;
        cooldownTxt.text   = text;
    }

    private static void SetActiveIfChanged(GameObject go, bool active)
    {
        if (go != null && go.activeSelf != active)
            go.SetActive(active);
    }
}