using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataRanking", menuName = "DataRanking")]
public class DataRankingController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] RankingList content;
    public RankingList ContentContent => content;
}

[Serializable]
public class RankingElementData
{
    public int id;
    public string name;
    public int point;
    public string nation;
    public int avatar;
}

[Serializable]
public class RankingList
{
    [SpreadsheetPage("Ranking")]
    public List<RankingElementData> rankinglist;
}