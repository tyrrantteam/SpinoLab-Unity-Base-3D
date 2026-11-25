using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataLuckySpin", menuName = "DataLuckySpin")]
public class DataLuckySpinController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listItemLuckySpin content;
    public listItemLuckySpin ContentContent => content;

    public int MileStoneSpin
    {
        get
        {
            if (content?.ListitemData != null && content.ListitemData.Count > 0)
            {
                for (int i = 0; i < content.ListitemData.Count; i++)
                {
                    if (content.ListitemData[i].MileStoneSpin > 0)
                        return content.ListitemData[i].MileStoneSpin;
                }
            }
            return 5;
        }
    }
}

[Serializable]
public class listItemLuckySpin
{
    [SpreadsheetPage("LuckySpin")]
    public List<ItemLuckySpin> ListitemData;
}

[Serializable]
public class ItemLuckySpin
{
    public int id;
    public string typeResources;
    public int value;
    public float rate;
    public int MileStoneSpin;
}
