using System;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using UnityEngine;

public static class DailyStreakService
{
    /// <summary>In-memory flag survives scene reload (LoadingScene) even if ES3 read lags.</summary>
    private static bool _pendingShowAfterWin;
    private static bool _isPopupOpen;

    public static bool IsPopupOpen => _isPopupOpen;

    public static void NotifyPopupClosed()
    {
        _isPopupOpen = false;
    }

    public static void OnLevelWon()
    {
        var config = GetConfig();
        if (config == null)
        {
            GameDebug.LogWarning("[DailyStreak] Config null — kiểm tra Assets/Resources/Data/DataDailyStreak.asset");
            return;
        }

        if (!config.isEnabled)
            return;

        var player = DataAccountPlayer.PlayerStreakData;

        if (ApplyCheatOnWin(config, player))
            return;

        if (player.hasClaimedFinalReward)
            return;

        _pendingShowAfterWin = true;
        player.MarkPendingShow();
        GameDebug.Log("[DailyStreak] Pending show on next level.");
    }

    public static bool TryShowPopup()
    {
        var config = GetConfig();
        if (config == null)
        {
            GameDebug.LogWarning("[DailyStreak] TryShow skipped: config null.");
            return false;
        }

        if (!config.isEnabled)
        {
            GameDebug.Log("[DailyStreak] TryShow skipped: isEnabled = false.");
            return false;
        }

        if (PopupManager.IsNull)
        {
            GameDebug.LogWarning("[DailyStreak] TryShow skipped: PopupManager null.");
            return false;
        }

        var player = DataAccountPlayer.PlayerStreakData;
        bool hasPending = _pendingShowAfterWin || player.pendingShowOnNextLevel;

        if (!hasPending)
        {
            GameDebug.Log("[DailyStreak] TryShow skipped: no pending flag.");
            return false;
        }

        if (player.hasClaimedFinalReward)
        {
            _pendingShowAfterWin = false;
            player.ClearPendingShow();
            GameDebug.Log("[DailyStreak] TryShow skipped: final reward already claimed.");
            return false;
        }

        int dayToCheckIn = ResolveDayToCheckIn(config, player);
        if (dayToCheckIn <= 0)
        {
            _pendingShowAfterWin = false;
            player.ClearPendingShow();
            GameDebug.Log("[DailyStreak] TryShow skipped: không cần check-in hôm nay.");
            return false;
        }

        var popup = PopupManager.Instance.ShowPopup<PopupDailyStreakController>(
            p => p.Setup(dayToCheckIn, config));

        if (popup == null)
        {
            GameDebug.LogWarning("[DailyStreak] TryShow failed: PopupDailyStreakController không có trong PopupManager.");
            return false;
        }

        _pendingShowAfterWin = false;
        player.ClearPendingShow();
        _isPopupOpen = true;
        GameDebug.Log($"[DailyStreak] Show popup — check-in day {dayToCheckIn}.");
        return true;
    }

    public static void CheatSetStreakDay(int targetDay)
    {
        var config = GetConfig();
        if (config == null)
            return;

        targetDay = Mathf.Clamp(targetDay, 1, config.streakDayCount);
        DataAccountPlayer.PlayerStreakData.SetCheatMoveToDay(targetDay);
    }

    public static void CheatFinishStreak()
    {
        if (GetConfig() == null)
            return;

        DataAccountPlayer.PlayerStreakData.SetCheatFinishStreak();
    }

    private static bool ApplyCheatOnWin(DataDailyStreak config, PlayerStreakData player)
    {
        switch (player.cheatMode)
        {
            case StreakCheatMode.MoveToDay:
                player.currentStreakDay = Mathf.Clamp(player.cheatTargetDay, 1, config.streakDayCount) - 1;
                player.lastCheckinDate = DateTime.UtcNow.AddDays(-1);
                player.ClearCheatMode();
                _pendingShowAfterWin = true;
                player.MarkPendingShow();
                return true;

            case StreakCheatMode.FinishStreak:
                player.currentStreakDay = Mathf.Max(config.streakDayCount - 1, 0);
                player.lastCheckinDate = DateTime.UtcNow.AddDays(-1);
                player.ClearCheatMode();
                _pendingShowAfterWin = true;
                player.MarkPendingShow();
                return true;

            default:
                return false;
        }
    }

    private static int ResolveDayToCheckIn(DataDailyStreak config, PlayerStreakData player)
    {
        if (player.currentStreakDay >= config.streakDayCount)
            return 0;

        DateTime today = DateTime.UtcNow.Date;

        if (player.currentStreakDay > 0 && player.lastCheckinDate.Date == today)
            return 0;

        if (player.currentStreakDay == 0)
            return 1;

        int daysSinceLastCheckin = (today - player.lastCheckinDate.Date).Days;
        if (daysSinceLastCheckin > 1)
        {
            player.ResetStreak();
            return 1;
        }

        if (daysSinceLastCheckin < 1)
            return 0;

        return player.currentStreakDay + 1;
    }

    private static DataDailyStreak GetConfig()
    {
        if (LoadResourceController.IsNull)
            return null;

        return LoadResourceController.Instance.DataDailyStreak();
    }
}
