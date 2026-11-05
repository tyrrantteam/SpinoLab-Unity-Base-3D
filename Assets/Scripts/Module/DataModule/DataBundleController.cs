using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Core.Debug;

[CreateAssetMenu(fileName = "DataBundle", menuName = "DataBundle")]
public class DataBundleController : SpreadsheetsContainerBase
{
    [SpreadsheetContent] [SerializeField] listBundle content;
    public                                listBundle ContentContent => content;

    [NonSerialized] private Dictionary<BundleType, List<List<BundleReward>>> _cachedBundleRewards;

    private void OnEnable()
    {
        InvalidateCache();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        InvalidateCache();
    }
#endif

    public void InvalidateCache()
    {
        _cachedBundleRewards = null;
    }

    public bool IsBundleAvailable(BundleType type)
    {
        return _cachedBundleRewards[type].Count > 0;
    }

    public List<List<BundleReward>> GetBundleRewardsByType(BundleType type)
    {
        if (_cachedBundleRewards == null)
        {
            BuildCache();
        }

        if (_cachedBundleRewards != null && _cachedBundleRewards.TryGetValue(type, out var list))
        {
            return list;
        }

        return null;
    }

    public void BuildCacheData() => BuildCache();
    
    private void BuildCache()
    {
        _cachedBundleRewards = new Dictionary<BundleType, List<List<BundleReward>>>();

        if (content == null)
        {
            return;
        }

        // 1. Bundle (packs 1 -> 6)
        var normalBundles = new List<List<BundleReward>>(6);
        AddPackIfActive(normalBundles, content.listBundlePack1);
        AddPackIfActive(normalBundles, content.listBundlePack2);
        AddPackIfActive(normalBundles, content.listBundlePack3);
        AddPackIfActive(normalBundles, content.listBundlePack4);
        AddPackIfActive(normalBundles, content.listBundlePack5);
        AddPackIfActive(normalBundles, content.listBundlePack6);
        _cachedBundleRewards[BundleType.Bundle] = normalBundles;

        // 2. Special Offer
        var specialList = new List<List<BundleReward>>(1);
        if (content.specialBundle != null && content.specialBundle.Count > 0)
        {
            specialList.Add(content.specialBundle);
        }

        _cachedBundleRewards[BundleType.SpecialOffer] = specialList;

        // 3. No Ads
        var noAdsList = new List<List<BundleReward>>(1);
        if (content.noAdsBundle1 != null && content.noAdsBundle1.Count > 0)
        {
            noAdsList.Add(content.noAdsBundle1);
        }

        _cachedBundleRewards[BundleType.NoAds] = noAdsList;

        // 4. VIP
        var vipList = new List<List<BundleReward>>(1);
        if (content.vipBundle3 != null && content.vipBundle3.Count > 0)
        {
            vipList.Add(content.vipBundle3);
        }

        _cachedBundleRewards[BundleType.VIP] = vipList;

        // 5. Coins: Mỗi row trong Bundle_packGold là 1 coin pack riêng
        var coinsList = new List<List<BundleReward>>();
        if (content.bundle_packGold != null)
        {
            for (int i = 0; i < content.bundle_packGold.Count; i++)
            {
                var reward = content.bundle_packGold[i];
                if (reward != null)
                {
                    coinsList.Add(new List<BundleReward>(1) { reward });
                }
            }
        }

        _cachedBundleRewards[BundleType.Coins] = coinsList;
    }

    private static void AddPackIfActive(List<List<BundleReward>> targetList, List<BundleReward> pack)
    {
        if (pack != null && pack.Count > 0 && pack[0].isActive)
        {
            targetList.Add(pack);
        }
    }
}

[Serializable]
public class BundleReward
{
    public int    id;
    public string typeResources;
    public int    value;
    public float  price;
    public bool   isRemoveAds;
    public bool   oneTimePurchase;
    public string idBundle;
    public string name;
    public string iconBundle;
    public int    levelUnlock;
    public int    hourActive;
    public int    limitPurchase;
    public int    timeActive;
    public bool   isActive;
}

[Serializable]
public class listBundle
{
    [SpreadsheetPage("Bundle_pack1")]    public List<BundleReward> listBundlePack1;
    [SpreadsheetPage("Bundle_pack2")]    public List<BundleReward> listBundlePack2;
    [SpreadsheetPage("Bundle_pack3")]    public List<BundleReward> listBundlePack3;
    [SpreadsheetPage("Bundle_pack4")]    public List<BundleReward> listBundlePack4;
    [SpreadsheetPage("Bundle_pack5")]    public List<BundleReward> listBundlePack5;
    [SpreadsheetPage("Bundle_pack6")]    public List<BundleReward> listBundlePack6;
    [SpreadsheetPage("Special_bundle")]  public List<BundleReward> specialBundle;
    [SpreadsheetPage("No_ads_bundle1")]  public List<BundleReward> noAdsBundle1;
    [SpreadsheetPage("No_ads_bundle2")]  public List<BundleReward> noAdsBundle2;
    [SpreadsheetPage("No_ads_bundle3")]  public List<BundleReward> noAdsBundle3;
    [SpreadsheetPage("Vip_bundle1")]     public List<BundleReward> vipBundle1;
    [SpreadsheetPage("Vip_bundle2")]     public List<BundleReward> vipBundle2;
    [SpreadsheetPage("Vip_bundle3")]     public List<BundleReward> vipBundle3;
    [SpreadsheetPage("Bundle_packGold")] public List<BundleReward> bundle_packGold;
}