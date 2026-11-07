using System;
using System.Collections.Generic;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using UnityEngine;
using Base.Core.Debug;

public abstract class BundleWithRewards : BundleBase
{
    public List<ModuleRewardController> rewards;

    /// <summary>
    /// Subclass override để cung cấp ModuleRewardController hiển thị gold.
    /// </summary>
    protected virtual ModuleRewardController GetGoldDisplay() => null;

    /// <summary>
    /// Subclass override để cung cấp ModuleRewardController hiển thị heart.
    /// </summary>
    protected virtual ModuleRewardController GetHeartDisplay() => null;

    public override void LoadBundle(List<BundleReward> reward, int index, bool isProduct)
    {
        base.LoadBundle(reward, index, isProduct);

        // Hook cho subclass xử lý trước vòng lặp (vd: set header text)
        OnBeforeParseRewards(reward);

        var boosterRewards = new List<BundleReward>();

        for (int i = 0; i < reward.Count; i++)
        {
            var initReward = reward[i];

            if (!Enum.TryParse<TypeResources>(initReward.typeResources, true, out var type))
            {
                GameDebug.LogError($"[BundleWithRewards] Wrong type: {initReward.typeResources}");
                continue;
            }

            switch (type)
            {
                case TypeResources.booster1:
                case TypeResources.booster2:
                case TypeResources.booster3:
                    boosterRewards.Add(initReward);
                    break;

                case TypeResources.gold:
                    GetGoldDisplay()?.InitData(new dataItem
                    {
                        typeReward = TypeResources.gold,
                        value      = initReward.value
                    });
                    break;

                case TypeResources.heart:
                    GetHeartDisplay()?.InitData(new dataItem
                    {
                        typeReward = TypeResources.heart,
                        value      = initReward.value
                    });
                    break;

                default:
                    // Hook cho subclass xử lý type mới
                    OnHandleCustomReward(initReward, type);
                    break;
            }
        }

        if (boosterRewards.Count > 0)
        {
            InitDataForReward(boosterRewards);
        }
    }

    /// <summary>
    /// Hook: Xử lý trước khi parse rewards (vd: set header text).
    /// </summary>
    protected virtual void OnBeforeParseRewards(List<BundleReward> reward)
    {
    }

    /// <summary>
    /// Hook: Xử lý các TypeResources không nằm trong switch mặc định.
    /// </summary>
    protected virtual void OnHandleCustomReward(BundleReward reward, TypeResources type)
    {
    }

    public void InitDataForReward(List<BundleReward> rewardList)
    {
        if (rewards == null || rewardList == null)
        {
            return;
        }

        var count = Mathf.Min(rewardList.Count, rewards.Count);
        for (var i = 0; i < count; i++)
        {
            if (rewards[i] == null)
            {
                continue;
            }

            if (!Enum.TryParse<TypeResources>(rewardList[i].typeResources, true, out var type))
            {
                continue;
            }

            rewards[i].gameObject.SetActive(true);
            rewards[i].InitData(new dataItem
            {
                typeReward = type,
                value      = rewardList[i].value
            });
        }

        for (var i = count; i < rewards.Count; i++)
        {
            if (rewards[i] != null)
            {
                rewards[i].gameObject.SetActive(false);
            }
        }
    }
}