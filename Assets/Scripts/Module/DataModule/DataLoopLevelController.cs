using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCurveLevel", menuName = "DaTa/DataCurveLevel")]

public class DataLoopLevelController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] ListDataCurve content;
    public ListDataCurve ContentContent => content;
    public int levelStartRandom => content.listCurveData[0].levelStartRandom;
    public int startRandom => content.listCurveData[0].startRandom;
    public int maximumLevelCurve => content.listCurveData[content.listCurveData.Count - 1].level;

    public int levelRandom(int currentLevel)
    {
        if(currentLevel >  maximumLevelCurve)
        {

        }
        else if (currentLevel >= levelStartRandom && currentLevel <= maximumLevelCurve)
        {
            var listCureve = content.listCurveData;
            int currentDiffuculty = 0;
            for (int i = 0; i < listCureve.Count; i++)
            {
                if (listCureve[i].level == currentLevel)
                {
                    currentDiffuculty = listCureve[i].difficulty;
                }
            }
        }
            return 0;
    }

}


[Serializable]
public class ListDataCurve
{
    [SpreadsheetPage("ListCurveLevel")]
    public List<ProcessCurveData> listCurveData = new List<ProcessCurveData>();
}



[Serializable]
public struct ProcessCurveData
{
    public int level;
    public int difficulty;
    public int startRandom;
    public int levelStartRandom;
}
