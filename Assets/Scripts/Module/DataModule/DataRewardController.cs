using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataRewardPopup", menuName = "DataRewardPopup")]
public class DataRewardController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listPopup content;
    public listPopup ContentContent => content;
}

[Serializable]
public class PopupReward
{
    public int id;
    public string typeResources;
    public int value;
}

[Serializable]
public class PopupRewardDailyCheckin
{
    public int day;
    public int id;
    public string typeResources;
    public int value;
}

[Serializable]
public class listPopup
{
    [SpreadsheetPage("PopupLevelUp")]
    public List<PopupReward> popupLevelUp;
    [SpreadsheetPage("PopupUnlock")]
    public List<PopupReward> listBundlePack2;
    [SpreadsheetPage("DailyCheckin")]
    public List<PopupRewardDailyCheckin> dailyCheckin;
}