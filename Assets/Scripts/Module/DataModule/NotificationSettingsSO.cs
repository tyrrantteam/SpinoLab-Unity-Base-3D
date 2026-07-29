using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ScriptableObject that stores all local Android notification configuration.
/// Create via: Assets > Create > Notification / Notification Settings
/// </summary>
[CreateAssetMenu(fileName = "NotificationSettings", menuName = "Notification/Notification Settings")]
public class NotificationSettingsSO : ScriptableObject, ISerializationCallbackReceiver
{
    public const string ResourcePath = "NotificationSettings";

    [Header("Channel")]
    [Tooltip("Unique channel ID used by Android (do not change at runtime).")]
    public string channelId = "game_default_channel";

    [Tooltip("Visible channel name shown in device Settings.")]
    public string channelName = "Game Notifications";

    [Tooltip("Visible channel description shown in device Settings.")]
    public string channelDescription = "Receive updates and reminders from the game.";

    [Header("Daily — fixed local times")]
    public List<NotificationDailySlotData> dailyTimeSlots = new List<NotificationDailySlotData>();

    [Header("After app close — delay from pause")]
    public List<NotificationAfterCloseData> afterAppClose = new List<NotificationAfterCloseData>();

    [SerializeField, HideInInspector, FormerlySerializedAs("notifications")]
    private List<NotificationEntryData> _legacyNotifications = new List<NotificationEntryData>();

    public void OnBeforeSerialize() { }
 
    public void OnAfterDeserialize() => MigrateLegacyIfNeeded();

    /// <summary>
    /// One-time style upgrade from the single &quot;notifications&quot; list. Safe on repeated loads when the asset on disk
    /// was never re-saved (idempotent: only imports legacy when the new lists are still empty).
    /// </summary>
    private void MigrateLegacyIfNeeded()
    {
        if (_legacyNotifications == null || _legacyNotifications.Count == 0)
            return;

        if (dailyTimeSlots.Count > 0 || afterAppClose.Count > 0)
        {
            _legacyNotifications.Clear();
            return;
        }

        foreach (var entry in _legacyNotifications)
        {
            if (entry.scheduleMode == NotificationScheduleMode.DailyDeviceClock)
            {
                dailyTimeSlots.Add(new NotificationDailySlotData
                {
                    hour               = entry.hour,
                    minute             = entry.minute,
                    title              = entry.title,
                    message            = entry.message,
                    repeating          = entry.repeating,
                    repeatIntervalDays = entry.repeating
                        ? Mathf.Max(1, Mathf.RoundToInt(entry.repeatIntervalHours / 24f))
                        : 1,
                    smallIcon          = entry.smallIcon,
                    largeIcon          = entry.largeIcon,
                });
            }
            else
            {
                afterAppClose.Add(new NotificationAfterCloseData
                {
                    hoursAfterClose     = entry.fireAfterHours,
                    extraMinutes        = 0,
                    title               = entry.title,
                    message             = entry.message,
                    repeating           = entry.repeating,
                    repeatIntervalHours = entry.repeatIntervalHours,
                    smallIcon           = entry.smallIcon,
                    largeIcon           = entry.largeIcon,
                });
            }
        }

        _legacyNotifications.Clear();
    }
}

/// <summary>Legacy schedule mode (only used for deserializing old assets).</summary>
public enum NotificationScheduleMode
{
    RelativeHoursAfterPause,
    DailyDeviceClock,
}

[Serializable]
public class NotificationEntryData
{
    /// <summary>Default matches legacy YAML that only specified fireAfterHours (no scheduleMode key).</summary>
    public NotificationScheduleMode scheduleMode = NotificationScheduleMode.RelativeHoursAfterPause;
    public string title = "Come back!";
    [TextArea(2, 4)] public string message = "Your adventure awaits!";
    [Min(0f)] public float fireAfterHours = 1f;
    [Range(0, 23)] public int hour = 11;
    [Range(0, 59)] public int minute;
    public bool repeating = true;
    [Min(0.1f)] public float repeatIntervalHours = 24f;
    public string smallIcon = "icon_0";
    public string largeIcon = "";
}

[Serializable]
public class NotificationDailySlotData
{
    [Tooltip("Hour (0–23), device local time.")]
    [Range(0, 23)]
    public int hour = 11;

    [Tooltip("Minute (0–59), device local time.")]
    [Range(0, 59)]
    public int minute;

    public string title = "Come back!";
    [TextArea(2, 4)]
    public string message = "Your adventure awaits!";

    [Tooltip("When enabled, repeats every Repeat Interval Days at this clock time.")]
    public bool repeating = true;

    [Tooltip("Repeat period in days (e.g. 1 = every day at this time).")]
    [Min(1)]
    public int repeatIntervalDays = 1;

    public string smallIcon = "icon_0";
    public string largeIcon = "";
}

[Serializable]
public class NotificationAfterCloseData
{
    [Tooltip("Delay in whole hours after the app goes to background.")]
    [Min(0f)]
    public float hoursAfterClose = 1f;

    [Tooltip("Extra minutes added after Hours After Close (0–59).")]
    [Range(0, 59)]
    public int extraMinutes;

    public string title = "Come back!";
    [TextArea(2, 4)]
    public string message = "We miss you!";

    [Tooltip("When enabled, repeats every Repeat Interval Hours.")]
    public bool repeating;

    [Tooltip("Hours between repeats when Repeating is enabled.")]
    [Min(0.1f)]
    public float repeatIntervalHours = 24f;

    public string smallIcon = "icon_0";
    public string largeIcon = "";
}
