using System;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiển thị countdown Special/Starter Bundle.
///
/// Countdown được lưu bằng UTC timestamp nên:
/// - Tắt GameObject vẫn chạy
/// - Đổi scene vẫn chạy
/// - Thoát game vẫn chạy
/// - Mở game lại vẫn tiếp tục
/// </summary>
public class SpecialBundleTimeText : MonoBehaviour
{
    [SerializeField] private TMP_Text timeTxt;

    [SerializeField] [Min(0)] private int durationHours = 24;

    private bool _didHidePack;

    /// <summary>Refresh() walks the bundle config every call, so it is polled instead of run per frame.</summary>
    private const float TickInterval = 0.2f;

    private float _nextTickTime;
    private int   _shownTotalSecs = -1;

    public int DurationHours => durationHours;

    private void OnEnable()
    {
        _shownTotalSecs = -1;
        _nextTickTime   = 0f;
        // Refresh();
    }

    private void OnApplicationPause(bool paused)
    {
        if (!paused)
            Refresh();
    }

    private void OnApplicationFocus(bool focused)
    {
        if (focused)
            Refresh();
    }

    private void Update()
    {
        if (Time.unscaledTime < _nextTickTime)
            return;

        _nextTickTime = Time.unscaledTime + TickInterval;
        Refresh();
    }

    public void Refresh()
    {
        if (timeTxt == null)
            return;

        if (!TryGetConfig(out var cfg) || cfg == null)
            return;

        int hours = durationHours;

        if (hours <= 0)
            hours = cfg.hourActive;

        if (hours <= 0)
            return;

        var data = DataAccountPlayer.PlayerResourceData;
        var shopData = DataAccountPlayer.PlayerShopData;

        if (data == null)
            return;

        int currentLv =
            DataAccountPlayer.PlayerPointProcessData != null
                ? DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen
                : 1;

        // ==========================================
        // 1. ĐẢM BẢO ĐÃ CÓ THỜI ĐIỂM BẮT ĐẦU
        // ==========================================

        shopData.EnsureStarterPackUnlockTimestamp(
            cfg.levelUnlock,
            currentLv);

        // ==========================================
        // 2. LẤY THỜI GIAN CÒN LẠI
        // ==========================================

        TimeSpan remaining =
            shopData.GetStarterPackRemaining(hours);

        // ==========================================
        // 3. KIỂM TRA PACK CÒN ACTIVE
        // ==========================================

        bool stillOpen =
            remaining.TotalSeconds > 0 &&
            shopData.IsStarterPackActive(
                cfg.levelUnlock,
                hours,
                cfg.limitPurchase,
                currentLv);

        if (!stillOpen)
        {
            if (_shownTotalSecs != 0)
            {
                _shownTotalSecs = 0;
                timeTxt.text    = "00:00:00";
            }

            HidePack();

            return;
        }

        _didHidePack = false;

        // ==========================================
        // 4. HIỂN THỊ COUNTDOWN
        // ==========================================

        int displayHours =
            remaining.Days * 24 +
            remaining.Hours;

        int totalSecs =
            displayHours * 3600 +
            remaining.Minutes * 60 +
            remaining.Seconds;

        if (totalSecs == _shownTotalSecs)
            return;

        _shownTotalSecs = totalSecs;

        timeTxt.text =
            $"{displayHours:D2}:" +
            $"{remaining.Minutes:D2}:" +
            $"{remaining.Seconds:D2}";
    }

    private void HidePack()
    {
        GameDebug.Log("Hide Pack");
        if (_didHidePack)
            return;

        _didHidePack = true;

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
        this.PostEvent(EventID.HideSpecialBundle);
        // JinGroup.Common.HomeUIManager
        //         .Instance?
        //         .RefreshStarterPackButton();
    }

    private static bool TryGetConfig(out BundleReward cfg)
    {
        cfg = null;

        var loader = LoadResourceController.Instance;

        if (loader == null)
            return false;

        var data = loader.DataBundleController();

        var special =
            data != null
                ? data.ContentContent?.specialBundle
                : null;

        if (special == null ||
            special.Count == 0)
        {
            return false;
        }

        cfg = special[0];

        return cfg != null;
    }
}