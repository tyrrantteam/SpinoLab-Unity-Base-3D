using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "DataBundle", menuName = "DataBundle")]
public class DataBundleController : SpreadsheetsContainerBase
{
    [SpreadsheetContent] [SerializeField] listBundle content;
    public                                listBundle ContentContent => content;

    private Dictionary<string, List<BundleReward>> bundleDict;
    
    private void OnEnable()
    {
        bundleDict = new Dictionary<string, List<BundleReward>>();
        var allBundles = GetAllBundle();
        foreach (var bundle in allBundles)
        {
            bundleDict.Add(bundle[0].idBundle, bundle);
        }
    }

    public bool IsNoAdsBundle(string id)
    {
        var bundle = GetBundleById(id);
        return bundle[0].isRemoveAds;
    }

    public List<BundleReward> GetBundleById(string id)
    {
        return bundleDict[id];
    }
    
    public List<List<BundleReward>> GetAllBundle()
    {
        List<List<BundleReward>> allLevels = new List<List<BundleReward>>();

        // Duyệt qua tất cả các Field trong class listLevel
        FieldInfo[] fields = typeof(listBundle).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            // Kiểm tra nếu field đó là List<Level> thì thêm vào danh sách
            if (field.FieldType == typeof(List<BundleReward>))
            {
                List<BundleReward> levelList = (List<BundleReward>)field.GetValue(content);
                if (levelList != null)
                {
                    allLevels.Add(levelList);
                }
            }
        }
        return allLevels;
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
    public string localizedPrice;
    public string productType;

    public void InitLocalPrice(string input)
    {
        localizedPrice = input;
    }
}

[Serializable]
public class listBundle
{
    [SpreadsheetPage("Bundle_pack1")]    public List<BundleReward> listBundlePack1;
    [SpreadsheetPage("Bundle_pack2")]    public List<BundleReward> listBundlePack2;
    [SpreadsheetPage("Bundle_packGold")] public List<BundleReward> bundle_packGold;
}