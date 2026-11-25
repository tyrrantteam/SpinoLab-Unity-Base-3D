using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using Base.Core.Debug;
using Unity.VisualScripting;
using DataAccount;
using Sirenix.OdinInspector;

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
    [ContextMenu("Auto Link Bundle Sprites")]
    private void AutoLinkAllBundles()
    { 
        InvalidateCache();
        AutoLinkSprites(false);
    }

    public const string BundleIconFolder = "Assets/Texture/UI/UI_shop/chest_icon";

    [ContextMenu("Auto Link Bundle Sprites")]
    public void AutoLinkSprites(bool saveAsset = true)
    {
        if (content == null) return;

        bool changed = false;
        changed |= LinkSprites(content.listBundlePack1);
        changed |= LinkSprites(content.listBundlePack2);
        changed |= LinkSprites(content.listBundlePack3);
        changed |= LinkSprites(content.listBundlePack4);
        changed |= LinkSprites(content.listBundlePack5);
        changed |= LinkSprites(content.listBundlePack6);
        changed |= LinkSprites(content.specialBundle);
        changed |= LinkSprites(content.noAdsBundle1);
        changed |= LinkSprites(content.noAdsBundle2);
        changed |= LinkSprites(content.noAdsBundle3);
        changed |= LinkSprites(content.vipBundle1);
        changed |= LinkSprites(content.vipBundle2);
        changed |= LinkSprites(content.vipBundle3);
        changed |= LinkSprites(content.bundle_packGold);
        changed |= LinkSprites(content.dailyBundle);

        if (changed)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            if (saveAsset)
            {
                UnityEditor.AssetDatabase.SaveAssets();
            }
            Debug.Log("<color=green>[DataBundleController]</color> Auto linked imgIconBundle for bundles successfully!");
        }
    }

    private static bool LinkSprites(List<BundleReward> list)
    {
        if (list == null || list.Count == 0) return false;

        bool changed = false;
        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            if (item == null) continue;

            var sprite = LoadSpriteAtPath(GetIconBundlePath(item.iconName));
            if (sprite != null && item.imgIconBundle != sprite)
            {
                item.imgIconBundle = sprite;
                changed = true;
            }
        }
        return changed;
    }

    public static string GetIconBundlePath(string iconBundle)
    {
        if (string.IsNullOrWhiteSpace(iconBundle)) return null;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(iconBundle.Trim());
        return $"{BundleIconFolder}/{fileName}.png";
    }

    private static Sprite LoadSpriteAtPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;

        var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets != null)
        {
            foreach (var a in assets)
            {
                if (a is Sprite s)
                    return s;
            }
        }
        return null;
    }
#endif

    public void InvalidateCache()
    {
        _cachedBundleRewards = null;
    }

    public bool IsBundleAvailable(BundleType type)
    {
        if (_cachedBundleRewards == null)
        {
            BuildCache();
        }

        if (_cachedBundleRewards != null && _cachedBundleRewards.TryGetValue(type, out var list))
        {
            return list != null && list.Count > 0;
        }

        return false;
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
        GameDebug.Log("start cached");
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
        Debug.Log($"Bundle count: {normalBundles.Count} -- {BundleType.Bundle}");

        // 2. Special Offer
        var specialList = new List<List<BundleReward>>(1);
        if (content.specialBundle != null && content.specialBundle.Count > 0)
        {
            specialList.Add(content.specialBundle);
        }

        _cachedBundleRewards[BundleType.SpecialOffer] = specialList;
        Debug.Log($"Bundle count: {specialList.Count} -- {BundleType.SpecialOffer}");

        // 3. No Ads
        var noAdsList = new List<List<BundleReward>>();
        AddPackIfActive(noAdsList, content.noAdsBundle1);
        AddPackIfActive(noAdsList, content.noAdsBundle2);
        AddPackIfActive(noAdsList, content.noAdsBundle3);
        _cachedBundleRewards[BundleType.NoAds] = noAdsList;
        Debug.Log($"Bundle count: {noAdsList.Count} -- {BundleType.NoAds}");
        // 4. VIP
        var vipList = new List<List<BundleReward>>();
        AddPackIfActive(vipList, content.vipBundle1);
        AddPackIfActive(vipList, content.vipBundle2);
        AddPackIfActive(vipList, content.vipBundle3);
        _cachedBundleRewards[BundleType.VIP] = vipList;
        Debug.Log($"Bundle count: {vipList.Count} -- {BundleType.VIP}");

        // 5. Coins: Mỗi row trong Bundle_packGold là 1 coin pack riêng
        var coinsList = new List<List<BundleReward>>();
        if (content.bundle_packGold != null)
        {
            for (int i = 0; i < content.bundle_packGold.Count; i++)
            {
                var reward = content.bundle_packGold[i];
                if (reward.isActive)
                {
                    coinsList.Add(new List<BundleReward>(1) { reward });
                }
            }
        }

        _cachedBundleRewards[BundleType.Coins] = coinsList;
        Debug.Log($"Bundle count: {coinsList.Count} -- {BundleType.Coins}");

        // 6. Daily Bundle
        var dailyBundle = new List<List<BundleReward>>();
        var todayDailyPack = GetDailyBundleForToday();
        if (todayDailyPack != null && todayDailyPack.Count > 0)
        {
            dailyBundle.Add(todayDailyPack);
        }
        _cachedBundleRewards[BundleType.Daily] = dailyBundle;
        Debug.Log($"Bundle count: {dailyBundle.Count} -- {BundleType.Daily}");
    }

    private void AddPackIfActive(List<List<BundleReward>> targetList, List<BundleReward> pack)
    {
        if (pack != null && pack.Count > 0 && pack[0].isActive)
        {
            targetList.Add(pack);
        }
        else if (pack != null && pack.Count > 0)
        {
            GameDebug.Log($"pack {pack[0].idBundle} was not active");
        }
    }

    public BundleReward GetDailyBundleRewardById(int id)
    {
        if (content?.dailyBundle == null) return null;
        return content.dailyBundle.Find(x => x.id == id);
    }

    public BundleReward RollDailyBundle()
    {
        if (content?.dailyBundle == null || content.dailyBundle.Count == 0)
        {
            GameDebug.LogWarning("[DataBundleController] dailyBundle list is empty!");
            return null;
        }

        var candidates = content.dailyBundle.FindAll(x => x.isActive && x.rate > 0);
        if (candidates.Count == 0)
        {
            candidates = content.dailyBundle.FindAll(x => x.isActive);
            if (candidates.Count == 0)
            {
                candidates = content.dailyBundle;
            }
        }

        int totalRate = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            totalRate += candidates[i].rate;
        }

        if (totalRate <= 0)
        {
            int index = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[index];
        }

        int randomValue = UnityEngine.Random.Range(0, totalRate);
        int currentWeight = 0;
        for (int i = 0; i < candidates.Count; i++)
        {
            currentWeight += candidates[i].rate;
            if (randomValue < currentWeight)
            {
                return candidates[i];
            }
        }

        return candidates[0];
    }

    public List<BundleReward> GetDailyBundleForToday()
    {
        if (content?.dailyBundle == null || content.dailyBundle.Count == 0)
        {
            return null;
        }

        var shopData = DataAccountPlayer.PlayerShopData;
        string todayUtc = DateTime.UtcNow.ToString("yyyy-MM-dd");

        BundleReward selected = null;
        if (shopData != null && shopData.dailyBundleDateUtc == todayUtc && shopData.dailyBundleRewardId > 0)
        {
            selected = GetDailyBundleRewardById(shopData.dailyBundleRewardId);
        }

        if (selected == null)
        {
            selected = RollDailyBundle();
            if (selected != null && shopData != null)
            {
                shopData.ResetDailyBundle(selected.id, todayUtc);
            }
        }

        if (selected == null)
        {
            return null;
        }

        // Support both single item and pack grouped by idBundle
        if (!string.IsNullOrEmpty(selected.idBundle))
        {
            var pack = content.dailyBundle.FindAll(x => x.isActive && x.idBundle == selected.idBundle);
            if (pack.Count > 0)
            {
                return pack;
            }
        }

        return new List<BundleReward>(1) { selected };
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
    public string iconName;
    public Sprite imgIconBundle;
    public string productType;
    public int    levelUnlock; 
    public int    hourActive;
    public int    limitPurchase;
    public int    timeActive;
    public bool   isActive;
    public int    rate;
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
    [SpreadsheetPage("Daily_bundle")]    public List<BundleReward> dailyBundle;
}
