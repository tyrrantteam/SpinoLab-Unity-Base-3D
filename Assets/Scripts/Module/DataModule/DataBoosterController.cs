using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataProcessMechanic;
[CreateAssetMenu(fileName = "DataBooster", menuName = "DaTa/DataBooster")]
public class DataBoosterController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] ListBooster content;
    public ListBooster ContentContent => content;

  
    public int BoosterCount => content.listBoooster?.Count ?? 0;

    public bool TryGetBoosterByLevel(int level, out ProcessBoosterData data)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.level == level)
            {
                data = t;
                return true;
            }
        }
        data = default;
        return false;
    }

    public ProcessBoosterData GetDataBoosterByType(BoosterType boosterType)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.boosterType == boosterType)
            {
                return t;
            }
        }
        return default;
    }

    public ProcessBoosterData GetDataBoosterByLevel(int level)
    {
        foreach (var t in content.listBoooster)
        {
            if (t.level == level)
            {
                return t;
            }
        }
        return default;
    }
}

[Serializable]
public class ListBooster
{
    [SpreadsheetPage("ListBooster")]
    public List<ProcessBoosterData> listBoooster = new List<ProcessBoosterData>();
}

[Serializable]
public struct ProcessBoosterData
{
    public int level;
    public BoosterType boosterType;
    [Header("icon Preview Booster")]
    public Sprite imgMechanic;
    [Header("icon Button Booster")]
    public Sprite imgIconBooster;
    public string description;
    public string nameBooster;
    public int price;
}

