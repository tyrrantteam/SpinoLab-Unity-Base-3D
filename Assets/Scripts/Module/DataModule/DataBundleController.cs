using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataBundle", menuName = "DataBundle")]
public class DataBundleController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listBundle content;
    public listBundle ContentContent => content;
}
[Serializable]
public class BundleReward
{
    public int id;
    public string typeResources;
    public int value;
    public float price;
    public bool isRemoveAds;
    public bool oneTimePurchase;
    public string idBundle;
    public string name;

}

[Serializable]
public class listBundle
{
    [SpreadsheetPage("Bundle_pack1")]
    public List<BundleReward> listBundlePack1;
    [SpreadsheetPage("Bundle_pack2")]
    public List<BundleReward> listBundlePack2;
    [SpreadsheetPage("Bundle_pack3")]
    public List<BundleReward> listBundlePack3;
    [SpreadsheetPage("Bundle_pack4")]
    public List<BundleReward> listBundlePack4;
    [SpreadsheetPage("Bundle_pack5")]
    public List<BundleReward> listBundlePack5;
    [SpreadsheetPage("Bundle_pack6")]
    public List<BundleReward> listBundlePack6;
    [SpreadsheetPage("Bundle_packGold")]
    public List<BundleReward> bundle_packGold;
}