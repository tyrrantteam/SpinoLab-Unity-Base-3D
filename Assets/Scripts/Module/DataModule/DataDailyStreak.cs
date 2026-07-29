using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataDailyStreak", menuName = "Data/Daily Streak")]
public class DataDailyStreak : ScriptableObject
{
    [Tooltip("Bật/tắt tính năng Daily Streak.")]
    public bool isEnabled;

    [Tooltip("Tổng số ngày streak (ngày cuối là gift).")]
    public int streakDayCount = 7;

    public Color colorCheckin = Color.white;
    public Color colorNormal = Color.gray;

    [Tooltip("Delay (giây) trước khi bật check-in streak trên ngày hiện tại.")]
    public int timeCastCheckinItem = 1;

    [Tooltip("Thời gian chờ (giây) sau khi check-in xong rồi đóng popup (trước ngày cuối).")]
    public float timeClose = 1.5f;

    [Tooltip("ID bộ phần thưởng được trao khi hoàn thành streak.")]
    public int finalRewardId = 1;

    public List<DailyStreakFinalReward> finalRewards = new();

    public bool TryGetRewardPack(int id, out DailyStreakFinalReward pack)
    {
        for (int i = 0; i < finalRewards.Count; i++)
        {
            if (finalRewards[i].id == id)
            {
                pack = finalRewards[i];
                return true;
            }
        }

        pack = default;
        return false;
    }

    public IReadOnlyList<DailyStreakRewardItem> GetFinalRewardItems()
    {
        if (TryGetRewardPack(finalRewardId, out var pack) && pack.items != null)
            return pack.items;

        return Array.Empty<DailyStreakRewardItem>();
    }
}

[Serializable]
public class DailyStreakFinalReward
{
    public int id;
    public List<DailyStreakRewardItem> items = new();
}

[Serializable]
public class DailyStreakRewardItem
{
    public BoosterType boosterType;
    public int boosterCount = 1;
}
