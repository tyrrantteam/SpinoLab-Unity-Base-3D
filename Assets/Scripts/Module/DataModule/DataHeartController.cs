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
            if (content?.HeartData != null && content.HeartData.limit > 0) return content.HeartData.limit;
            return 5;
        }
    }

    public float RecoveryTime
    {
        get
        {
            if (HeartData != null && HeartData.time > 0) return HeartData.time;
            if (content?.HeartData != null && content.HeartData.time > 0) return content.HeartData.time;
            return 600f;
        }
    }

    public int PriceHeart
    {
        get
        {
            if (HeartData != null && HeartData.priceHeart > 0) return HeartData.priceHeart;
            if (content?.HeartData != null && content.HeartData.priceHeart > 0) return content.HeartData.priceHeart;
            return 190;
        }
    }

    public int ValueHeart
    {
        get
        {
            if (HeartData != null && HeartData.valueHeart > 0) return HeartData.valueHeart;
            if (content?.HeartData != null && content.HeartData.valueHeart > 0) return content.HeartData.valueHeart;
            return Limit > 0 ? Limit : 5;
        }
    }

    public int priceHeart => PriceHeart;
    public int valueHeart => ValueHeart;
}

[Serializable]
public class HeartDataConfig
{
    [SpreadsheetPage("HeartConfig")]
    public HeartData HeartData;
}

[Serializable]
public class HeartData
{
    public float time;
    public int limit;
    public float unlimitday;
    public int priceHeart;
    public int valueHeart;
}
