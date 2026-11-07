using System;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using TMPro;

public class VipBundle : BundleWithRewards
{
    public ModuleRewardController incomes;
    public ModuleRewardController goldNumber;
    public ModuleRewardController slotDocker;
    public TMP_Text               vipTextHeader;

    protected override ModuleRewardController GetGoldDisplay() => goldNumber;
    protected override void OnGrantProducts() => GrantAllRewards();

    protected override void OnBeforeParseRewards(List<BundleReward> reward)
    {
        if (vipTextHeader != null)
        {
            vipTextHeader.text = reward[0].name;
        }
    }

    protected override void OnHandleCustomReward(BundleReward reward, TypeResources type)
    {
        if (incomes == null)
        {
            return;
        }

        if (type != TypeResources.incomex2
            && type != TypeResources.incomex3
            && type != TypeResources.incomex4)
        {
            return;
        }

        incomes.gameObject.SetActive(true);
        incomes.InitData(new dataItem
        {
            typeReward = type,
            value      = reward.value
        });
    }

    protected override void GrantAllRewards()
    {
        int days       = GetVipDays();
        int multiplier = GetIncomeMultiplier();
        base.GrantAllRewards();
        DataAccountPlayer.PlayerResourceData.SaveTimeLineUnlock(days, multiplier);
    }

    private int GetVipDays()
    {
        if (_bundleRewards == null || _bundleRewards.Count == 0)
        {
            return 0;
        }

        return _bundleRewards[0].timeActive;
    }

    private int GetIncomeMultiplier()
    {
        if (_bundleRewards == null)
        {
            return 1;
        }

        foreach (var reward in _bundleRewards)
        {
            if (!Enum.TryParse(reward.typeResources, true, out TypeResources type))
            {
                continue;
            }

            switch (type)
            {
                case TypeResources.incomex4: return 4;
                case TypeResources.incomex3: return 3;
                case TypeResources.incomex2: return 2;
            }
        }

        return 1;
    }
}
