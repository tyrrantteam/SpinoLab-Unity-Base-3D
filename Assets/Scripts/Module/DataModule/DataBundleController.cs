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
    [SpreadsheetPage("Bundle_packGold")]
    public List<BundleReward> bundle_packGold;
}