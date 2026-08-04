using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataWinReward", menuName = "DataWinReward")]

public class DataWinReward : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listWinReward content;
    public listWinReward ContentContent => content;
}

[Serializable]
public class WinReward
{
    public int    level;
    public int    id;
    public string typeResources;
    public int    value;

}

[Serializable]
public class listWinReward
{
    [SpreadsheetPage("winPopup")]
    public List<WinReward> listBundlePack1;
}