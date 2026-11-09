using System;
using System.Collections.Generic;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Module.Reward;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyBundle : BundleWithRewards
{
    public ModuleRewardController goldNumber;
    public ModuleRewardController heartNumber;
    public TMP_Text               timeTxt;
    public GameObject             claimedOverlay;

    [SerializeField] private const string freeText    = "FREE";
    [SerializeField] private const string claimedText = "CLAIMED";

    private const float  TickInterval = 0.5f;
    private       float  _nextTickTime;
    private       int    _lastShownSeconds = -1;
    private       string _currentLoadedDate;

    protected override ModuleRewardController GetGoldDisplay() => goldNumber;
    protected override ModuleRewardController GetHeartDisplay() => heartNumber;
    protected override void OnGrantProducts() => GrantAllRewards();

    protected override void Awake()
    {
        base.Awake();
        _currentLoadedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
    }

    private void OnEnable()
    {
        _nextTickTime     = 0f;
        _lastShownSeconds = -1;
        UpdateClaimStateUI();
        UpdateTimer();
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextTickTime)
        {
            return;
        }

        _nextTickTime = Time.unscaledTime + TickInterval;
        UpdateTimer();
    }

    public override void LoadBundle(List<BundleReward> reward, int index, bool isProduct)
    {
        base.LoadBundle(reward, index, isProduct);
        _currentLoadedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        UpdateClaimStateUI();
    }

    private void UpdateClaimStateUI()
    {
        var shopData = DataAccountPlayer.PlayerShopData;
        bool isClaimed = shopData != null && shopData.IsDailyBundleClaimedToday();

        if (btnBuy != null)
        {
            btnBuy.interactable = !isClaimed;
        }

        if (bundlePriceText != null)
        {
            bundlePriceText.text = isClaimed ? claimedText : freeText;
        }

        if (claimedOverlay != null)
        {
            claimedOverlay.SetActive(isClaimed);
        }
    }

    protected override void OnStartPurchase()
    {
        var shopData = DataAccountPlayer.PlayerShopData;
        if (shopData != null && shopData.IsDailyBundleClaimedToday())
        {
            GameDebug.Log("[DailyBundle] Already claimed today!");
            return;
        }

        if (!AdsManager.IsNull)
        {
            AdsManager.Instance.ShowRewarded(() => { OnClaimSuccess(); });

#if UNITY_EDITOR
            // Fallback for editor testing when ad network callback is simulated
            if (DataAccountPlayer.PlayerResourceData.skipAds <= 0)
            {
                OnClaimSuccess();
            }
#endif
        }
        else
        {
            OnClaimSuccess();
        }
    }

    private void OnClaimSuccess()
    {
        var shopData = DataAccountPlayer.PlayerShopData;
        if (shopData != null)
        {
            shopData.MarkDailyBundleClaimed();
        }

        ShowClaimRewardPopup();
        UpdateClaimStateUI();
    }

    private void UpdateTimer()
    {
        DateTime now = DateTime.UtcNow;
        string todayUtc = now.ToString("yyyy-MM-dd");

        // Check if 0h00 UTC has passed and day changed
        if (!string.IsNullOrEmpty(_currentLoadedDate) && _currentLoadedDate != todayUtc)
        {
            OnMidnightReset();
            return;
        }

        DateTime nextResetUtc = now.Date.AddDays(1);
        TimeSpan remaining = nextResetUtc - now;

        if (remaining.TotalSeconds <= 0)
        {
            OnMidnightReset();
            return;
        }

        if (timeTxt != null)
        {
            int totalSeconds = (int)remaining.TotalSeconds;
            if (totalSeconds != _lastShownSeconds)
            {
                _lastShownSeconds = totalSeconds;
                int hours = (int)remaining.TotalHours;
                timeTxt.text = $"{hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            }
        }
    }

    private void OnMidnightReset()
    {
        GameDebug.Log("[DailyBundle] Midnight reset (0h00 UTC+0) triggered!");
        _currentLoadedDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        _lastShownSeconds  = -1;

        var loader = LoadResourceController.Instance;
        var dataBundleController = loader != null ? loader.DataBundleController() : null;
        if (dataBundleController != null)
        {
            dataBundleController.InvalidateCache();
            var newReward = dataBundleController.GetDailyBundleForToday();
            if (newReward != null && newReward.Count > 0)
            {
                LoadBundle(newReward, 0, false);
            }
        }

        UpdateClaimStateUI();
    }
}