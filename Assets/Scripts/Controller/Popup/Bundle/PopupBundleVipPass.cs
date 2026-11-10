using System;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using UnityEngine;
using UnityEngine.UI;

public class PopupBundleVipPass : PopupBundleBaseController
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
    }
    
    protected override void InitData()
    {
        var data = LoadResourceController.Instance.DataBundleController();
        bundleData = data.ContentContent.vipBundle3;
        base.InitData();
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
        if (bundleData == null || bundleData.Count == 0)
        {
            return 0;
        }

        return bundleData[0].timeActive;
    }

    private int GetIncomeMultiplier()
    {
        if (bundleData == null)
        {
            return 1;
        }

        foreach (var reward in bundleData)
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