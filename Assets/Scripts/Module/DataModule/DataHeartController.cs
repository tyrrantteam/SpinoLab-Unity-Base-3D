using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "DataHeart", menuName = "DataHeart")]
public class DataHeartController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] HeartDataConfig content = new HeartDataConfig();
    public HeartDataConfig ContentContent => content;

    public HeartData HeartData => content?.HeartData;
    public int Limit
    {
        get
        {
            if (HeartData != null && HeartData.limit > 0) return HeartData.limit;
            if (content != null && content.limit > 0) return content.limit;
            return 5;
        }
    }
    public float RecoveryTime
    {
        get
        {
            if (HeartData != null && HeartData.time > 0) return HeartData.time;
            if (content != null && content.time > 0) return content.time;
            return 600f;
        }
    }
}

[Serializable]
public class HeartDataConfig
{
    [SpreadsheetPage("HeartConfig")]
    public HeartData HeartData;
    public float time;
    public int limit;
    public float unlimitday;
}

[Serializable]
public class HeartData
{
    public float time;
    public int limit;
    public float unlimitday;
}