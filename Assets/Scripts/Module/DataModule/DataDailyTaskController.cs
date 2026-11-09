using NorskaLib.Spreadsheets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataDailyTask", menuName = "DataDailyTask")]
public class DataDailyTaskController : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField] listItemDailyTask content;
    public listItemDailyTask ContentContent => content;

    [HideInInspector] public TimeSpan timeCountingReset;
    [HideInInspector] public string resetTimeDisplay;

    public ItemDailyTask itemDailyTask(TypeTaskDaily typeTask)
    {
        if (content?.listDailyTask != null)
        {
            for (int i = 0; i < content.listDailyTask.Count; i++)
            {
                if (content.listDailyTask[i].typeTask == typeTask)
                    return content.listDailyTask[i];
            }
        }
        return null;
    }

    public TimeSpan CalculateTimeUntilReset()
    {
        DateTime now = DateTime.Now;
        DateTime tomorrowMidnight = now.Date.AddDays(1);
        timeCountingReset = tomorrowMidnight - now;
        resetTimeDisplay = string.Format("Reset After : {0:D2}:{1:D2}:{2:D2}",
            (int)timeCountingReset.TotalHours,
            timeCountingReset.Minutes,
            timeCountingReset.Seconds);
        return timeCountingReset;
    }
}

[Serializable]
public class listItemDailyTask
{
    [SpreadsheetPage("DailyTask")]
    public List<ItemDailyTask> listDailyTask;
}

[Serializable]
public class ItemDailyTask
{
    public TypeTaskDaily typeTask;
    public int valueTask;
    public string typeResources;
    public int value;
    public string describeText;
}
