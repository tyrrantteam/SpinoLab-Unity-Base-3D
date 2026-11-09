using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyManager : SingletonMono<NotifyManager>
{
    public static void RefreshAllNoti()
    {
        var buttons = FindObjectsOfType<BaseFeatureHomeBtn>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null) buttons[i].InitDataNoti();
        }
    }

    public bool InitNotify(NotifyType typeNoti)
    {
        if (typeNoti == NotifyType.LuckySpin)
        {
            var dataCondition = DataAccountPlayer.PlayerLuckySpinData.currentLevelProcess;
            var dataValue = LoadResourceController.Instance != null 
                ? LoadResourceController.Instance.DataLuckySpinController().MileStoneSpin 
                : 5;
            if (dataCondition >= dataValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (typeNoti == NotifyType.DailyTask)
        {
            var dataDailyTask = LoadResourceController.Instance.DataDailyTaskController();

            if (dataDailyTask != null && dataDailyTask.ContentContent?.listDailyTask != null)
            {
                var tasks = dataDailyTask.ContentContent.listDailyTask;
                for (int i = 0; i < tasks.Count; i++)
                {
                    if (DataAccountPlayer.PlayerDailyTashkData.CanClaimTask(tasks[i].typeTask))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else if (typeNoti == NotifyType.DailyBundle)
        {
            return true;
        }
        else if (typeNoti == NotifyType.HiddenHat)
        {
            return true;
        }
        return false;
    }
}
