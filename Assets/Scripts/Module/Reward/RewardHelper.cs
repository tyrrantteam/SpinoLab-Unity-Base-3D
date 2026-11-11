using System;
using System.Collections.Generic;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Module.Reward;

namespace JinGroup.Module.Resources
{
    /// <summary>
    /// Single place for granting rewards to the account and showing reward slots in UI.
    /// Add new TypeResources handling here only.
    /// </summary>
    public static class RewardHelper
    {
        public static bool TryParseType(string typeResources, out TypeResources type)
        {
            return Enum.TryParse(typeResources, true, out type);
        }

        public static bool Grant(string typeResources, int value)
        {
            if (!TryParseType(typeResources, out var type))
            {
                GameDebug.LogWarning($"[RewardHelper] Unknown typeResources: {typeResources}");
                return false;
            }

            return Grant(type, value);
        }

        public static bool Grant(TypeResources type, int value)
        {
            var res = DataAccountPlayer.PlayerResourceData;
            if (res == null) return false;

            switch (type)
            {
                case TypeResources.gold:
                    res.ChangeGoldValue(value);
                    return true;
                case TypeResources.diamond:
                    res.ChangeDiamondValue(value);
                    return true;
                case TypeResources.skipAds:
                    res.ChangeSkipAdsValue(value);
                    return true;
                case TypeResources.heart:
                    res.ChangeHeartValue(value);
                    return true;
                case TypeResources.heartHour:
                    res.AddInfiniteHeartHours(value);
                    return true;
                case TypeResources.heartDay:
                    res.AddInfiniteHeartDays(value);
                    return true;
                case TypeResources.booster1:
                    res.ChangeBoosterCount(BoosterType.booster1, value);
                    return true;
                case TypeResources.booster2:
                    res.ChangeBoosterCount(BoosterType.booster2, value);
                    return true;
                case TypeResources.booster3:
                    res.ChangeBoosterCount(BoosterType.booster3, value);
                    return true;
                case TypeResources.noads:
                    res.ChangeNoAdsStatus(true);
                    return true;
                case TypeResources.noads24h:
                    res.ChangeNoAds24hStatus(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    return true;
                default:
                    GameDebug.LogWarning($"[RewardHelper] TypeResources not handled: {type}");
                    return false;
            }
        }

        public static void GrantAll(IReadOnlyList<BundleReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                GameDebug.LogWarning("[RewardHelper] Không có dữ liệu phần thưởng!");
                return;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                if (reward == null) continue;

                GameDebug.Log($"- Item ID: {reward.id} | Cần cộng: {reward.value} [{reward.typeResources}]");
                Grant(reward.typeResources, reward.value);
            }
        }

        /// <summary>Fills reward slots in order and hides unused slots.</summary>
        public static void ShowRewards(IReadOnlyList<ModuleRewardController> slots, IReadOnlyList<BundleReward> rewards)
        {
            if (slots == null) return;

            int count = rewards == null ? 0 : Math.Min(rewards.Count, slots.Count);
            for (int i = 0; i < count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                var reward = rewards[i];
                if (reward == null || !TryParseType(reward.typeResources, out var type))
                {
                    GameDebug.LogWarning($"[RewardHelper] Wrong type of type resources {reward?.typeResources}");
                    slot.gameObject.SetActive(false);
                    continue;
                }

                slot.gameObject.SetActive(true);
                slot.InitData(new dataItem
                {
                    typeReward = type,
                    value      = reward.value
                });
            }

            for (int i = count; i < slots.Count; i++)
            {
                if (slots[i] != null)
                    slots[i].gameObject.SetActive(false);
            }
        }
    }
}
