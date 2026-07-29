
using JinGroup.Module.Resources;
using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataItem", menuName = "DataItem")]
public class DataItemController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listItemData content;
    public listItemData ContentContent => content;
}
[Serializable]
public class ItemData
{
    public int id;
    public string typeResources;
    public string typeRarity;
    public string description;

}

[Serializable]
public class listItemData
{
    [SpreadsheetPage("DataItem")]
    public List<ItemData> ListitemData;
}