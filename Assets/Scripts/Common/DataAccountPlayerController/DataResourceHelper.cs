using Base.Core;
using DataAccount;
using JinGroup.Module.Resources;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class DataResourceHelper 
{
    public static void ClaimRewardDaily(List<PopupRewardDailyCheckin> itemData)
    {
        foreach (var item in itemData)
        {
            TypeResources typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), item.typeResources, true);
            CalaculateReward(typeReward, item.value);
        }
    }


    public static void ClaimReward(List<BundleReward> itemData)
    {
        foreach (var item in itemData)
        {
            TypeResources typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), item.typeResources, true);
            CalaculateReward(typeReward, item.value);
        }
    }

    public static void ClaimRewardPopup(List<PopupReward> itemData)
    {
        foreach (var item in itemData)
        {
            TypeResources typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), item.typeResources, true);
            CalaculateReward(typeReward, item.value);
        }
    }

    private static void CalaculateReward(TypeResources typeReward, int value)
    {
        if (typeReward == TypeResources.gold)
        {
            DataAccountPlayer.PlayerResourceData.ChangeGoldValue(value);
            Debug.Log(DataAccountPlayer.PlayerResourceData.gold + " Data gold");
        }
        else if (typeReward == TypeResources.diamond)
        {
            DataAccountPlayer.PlayerResourceData.ChangeDiamondValue(value);
            Debug.Log(DataAccountPlayer.PlayerResourceData.diamond + " Data diamond");
        }
        else if (typeReward == TypeResources.skipAds)
        {
            DataAccountPlayer.PlayerResourceData.ChangeSkipAdsValue(value);
            Debug.Log(DataAccountPlayer.PlayerResourceData.skipAds + " Data skipAds");
        }
    }
}
