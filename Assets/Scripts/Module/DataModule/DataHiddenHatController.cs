using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataHiddenHat", menuName = "DataHiddenHat")]
public class DataHiddenHatController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listItemHiddenHat content;
    public listItemHiddenHat ContentContent => content;

    private Dictionary<int, List<ItemHiddenHat>> _groupedItems;
    private int _cachedItemCount = -1;

    private void OnEnable()
    {
        BuildGroupCache();
    }

    /// <summary>
    /// Gom nhóm các ItemHiddenHat có trùng id lại thành từng list theo id group.
    /// </summary>
    public void BuildGroupCache()
    {
        if (_groupedItems == null)
            _groupedItems = new Dictionary<int, List<ItemHiddenHat>>();
        else
            _groupedItems.Clear();

        var sourceList = content != null ? content.listItemHiddenHatElement : null;
        if (sourceList == null)
        {
            _cachedItemCount = 0;
            return;
        }

        _cachedItemCount = sourceList.Count;
        for (int i = 0; i < sourceList.Count; i++)
        {
            var item = sourceList[i];
            if (item == null) continue;

            if (!_groupedItems.TryGetValue(item.id, out var groupList))
            {
                groupList = new List<ItemHiddenHat>();
                _groupedItems.Add(item.id, groupList);
            }

            groupList.Add(item);
        }
    }

    private void EnsureGroupCache()
    {
        int currentCount = content != null && content.listItemHiddenHatElement != null
            ? content.listItemHiddenHatElement.Count
            : 0;

        if (_groupedItems == null || _cachedItemCount != currentCount)
        {
            BuildGroupCache();
        }
    }

    /// <summary>
    /// Lấy toàn bộ dictionary đã group theo id.
    /// </summary>
    public Dictionary<int, List<ItemHiddenHat>> GetGroupedItems()
    {
        EnsureGroupCache();
        return _groupedItems;
    }

    /// <summary>
    /// Lấy danh sách tất cả id group hiện có.
    /// </summary>
    public List<int> GetAllGroupIds()
    {
        EnsureGroupCache();
        return new List<int>(_groupedItems.Keys);
    }

    /// <summary>
    /// Truyền vào id group để lấy ra danh sách reward tương ứng của group đó.
    /// </summary>
    public List<ItemHiddenHat> GetRewardsByGroupId(int groupId)
    {
        EnsureGroupCache();
        if (_groupedItems.TryGetValue(groupId, out var groupList))
        {
            return new List<ItemHiddenHat>(groupList);
        }

        return new List<ItemHiddenHat>();
    }

    public int GetPriceClaimAll(int groupId)
    {
        EnsureGroupCache();
        if (_groupedItems.TryGetValue(groupId, out var groupList))
        {
            for (int i = 0; i < groupList.Count; i++)
            {
                if (groupList[i] != null && groupList[i].priceClaimAll > 0)
                    return groupList[i].priceClaimAll;
            }
        }

        return 0;
    }

    public float GetTimeHours(int groupId, float defaultHours = 24f)
    {
        EnsureGroupCache();
        if (_groupedItems.TryGetValue(groupId, out var groupList))
        {
            for (int i = 0; i < groupList.Count; i++)
            {
                if (groupList[i] != null && groupList[i].time > 0f)
                    return groupList[i].time;
            }
        }

        return defaultHours;
    }
}

[Serializable]
public class listItemHiddenHat
{
    [SpreadsheetPage("HiddenHat")]
    public List<ItemHiddenHat> listItemHiddenHatElement;
}


[Serializable]
public class ItemHiddenHat
{
    public int id;
    public string typeResources;
    public int value;
    public int priceClaimAll;
    public float time;
}
