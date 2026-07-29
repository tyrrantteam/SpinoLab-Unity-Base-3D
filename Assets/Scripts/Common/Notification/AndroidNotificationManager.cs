using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

/// <summary>
/// Manages local Android notifications.
/// Attach to a persistent GameObject in your first scene (e.g. GameController).
/// Reads notification data from NotificationSettingsSO stored in Resources/.
/// Foreground: all scheduled local notifications are cancelled so nothing fires while the app is open.
/// </summary>
public class AndroidNotificationManager : MonoBehaviour
{
    private static AndroidNotificationManager _instance;

    [Tooltip("Override the SO loaded from Resources. Leave empty to auto-load."), SerializeField]
    private NotificationSettingsSO settingsOverride;

    private NotificationSettingsSO _settings;

    private readonly List<int> _scheduledIds = new List<int>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _settings = settingsOverride != null
            ? settingsOverride
            : Resources.Load<NotificationSettingsSO>(NotificationSettingsSO.ResourcePath);

        if (_settings == null)
        {
            Debug.LogWarning("[AndroidNotificationManager] NotificationSettingsSO not found. " +
                             $"Place it at Resources/{NotificationSettingsSO.ResourcePath}.asset");
            return;
        }

#if UNITY_ANDROID
        RequestPermission();
        RegisterChannel();
#endif
    }

    private void OnApplicationPause(bool paused)
    {
        if (_settings == null) return;

        if (paused)
            ScheduleAll();
#if UNITY_ANDROID
        else
            CancelAll();
#endif
    }

#if UNITY_ANDROID
    private void OnApplicationFocus(bool hasFocus)
    {
        if (_settings == null || !hasFocus) return;
        CancelAll();
    }
#endif

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Schedules daily slots and after-close rules from the ScriptableObject.</summary>
    public void ScheduleAll()
    {
#if UNITY_ANDROID
        CancelAll();

        if (_settings == null) return;

        foreach (var slot in _settings.dailyTimeSlots)
        {
            if (string.IsNullOrWhiteSpace(slot.title) && string.IsNullOrWhiteSpace(slot.message))
                continue;

            if (!TryBuildDailySlot(slot, out var notification))
                continue;

            Send(notification);
        }

        foreach (var rule in _settings.afterAppClose)
        {
            if (string.IsNullOrWhiteSpace(rule.title) && string.IsNullOrWhiteSpace(rule.message))
                continue;

            if (!TryBuildAfterClose(rule, out var notification))
                continue;

            Send(notification);
        }
#endif
    }

    /// <summary>Cancels all notifications scheduled through this manager.</summary>
    public void CancelAll()
    {
#if UNITY_ANDROID
        foreach (int id in _scheduledIds)
            AndroidNotificationCenter.CancelNotification(id);

        _scheduledIds.Clear();
#endif
    }

#if UNITY_ANDROID
    private void Send(AndroidNotification notification)
    {
        int id = AndroidNotificationCenter.SendNotification(notification, _settings.channelId);
        _scheduledIds.Add(id);
    }

    private static bool TryBuildDailySlot(NotificationDailySlotData slot, out AndroidNotification notification)
    {
        var now = DateTime.Now;
        var fireTime = new DateTime(now.Year, now.Month, now.Day, slot.hour, slot.minute, 0, DateTimeKind.Local);

        if (fireTime <= now)
            fireTime = fireTime.AddDays(1);

        notification = new AndroidNotification
        {
            Title    = slot.title,
            Text     = slot.message,
            FireTime = fireTime,
        };

        if (!string.IsNullOrEmpty(slot.smallIcon))
            notification.SmallIcon = slot.smallIcon;

        if (!string.IsNullOrEmpty(slot.largeIcon))
            notification.LargeIcon = slot.largeIcon;

        if (slot.repeating && slot.repeatIntervalDays > 0)
            notification.RepeatInterval = TimeSpan.FromDays(slot.repeatIntervalDays);

        return true;
    }

    private static bool TryBuildAfterClose(NotificationAfterCloseData rule, out AndroidNotification notification)
    {
        var delay = TimeSpan.FromHours(rule.hoursAfterClose) + TimeSpan.FromMinutes(rule.extraMinutes);
        if (delay <= TimeSpan.Zero)
        {
            notification = default;
            return false;
        }

        var fireTime = DateTime.Now + delay;

        notification = new AndroidNotification
        {
            Title    = rule.title,
            Text     = rule.message,
            FireTime = fireTime,
        };

        if (!string.IsNullOrEmpty(rule.smallIcon))
            notification.SmallIcon = rule.smallIcon;

        if (!string.IsNullOrEmpty(rule.largeIcon))
            notification.LargeIcon = rule.largeIcon;

        if (rule.repeating)
            notification.RepeatInterval = TimeSpan.FromHours(rule.repeatIntervalHours);

        return true;
    }

    private void RequestPermission()
    {
#if UNITY_2022_2_OR_NEWER
        _ = new PermissionRequest();
#endif
    }

    private void RegisterChannel()
    {
        var channel = new AndroidNotificationChannel
        {
            Id          = _settings.channelId,
            Name        = _settings.channelName,
            Description = _settings.channelDescription,
            Importance  = Importance.Default,
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
    }
#endif
}
