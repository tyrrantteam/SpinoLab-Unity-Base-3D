using System;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Base.LoadData;
using UnityEngine;

[Serializable]
public class DailyTaskProgress
{
    public TypeTaskDaily typeTask;
    public int currentValue;
    public bool isClaimed;
}

[Serializable]
public class PlayerDailyTashkData
{
    public string lastResetDate;
    public List<DailyTaskProgress> listTaskProgress = new List<DailyTaskProgress>();

    public bool CheckAndResetNewDay()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (string.IsNullOrEmpty(lastResetDate) || lastResetDate != today)
        {
            ResetAllTasks(today);
            return true;
        }
        return false;
    }

    public void ResetAllTasks(string newDate = null)
    {
        lastResetDate = newDate ?? DateTime.Now.ToString("yyyy-MM-dd");
        if (listTaskProgress == null)
            listTaskProgress = new List<DailyTaskProgress>();

        for (int i = 0; i < listTaskProgress.Count; i++)
        {
            listTaskProgress[i].currentValue = 0;
            listTaskProgress[i].isClaimed = false;
        }

        DataAccountPlayer.SavePlayerDailyTashkData();
    }

    public DailyTaskProgress GetTaskProgress(TypeTaskDaily typeTask)
    {
        CheckAndResetNewDay();

        if (listTaskProgress == null)
            listTaskProgress = new List<DailyTaskProgress>();

        var task = listTaskProgress.Find(t => t.typeTask == typeTask);
        if (task == null)
        {
            task = new DailyTaskProgress { typeTask = typeTask, currentValue = 0, isClaimed = false };
            listTaskProgress.Add(task);
            DataAccountPlayer.SavePlayerDailyTashkData();
        }
        return task;
    }

    public int GetCurrentValue(TypeTaskDaily typeTask)
    {
        return GetTaskProgress(typeTask).currentValue;
    }

    public bool IsTaskClaimed(TypeTaskDaily typeTask)
    {
        return GetTaskProgress(typeTask).isClaimed;
    }

    public void SetTaskClaimed(TypeTaskDaily typeTask, bool claimed = true)
    {
        var task = GetTaskProgress(typeTask);
        task.isClaimed = claimed;
        DataAccountPlayer.SavePlayerDailyTashkData();
        NotifyManager.RefreshAllNoti();
    }

    /// <summary>
    /// Cập nhật data các task truyền vào TypeTaskDaily mỗi khi call hàm này thì task đó sẽ được +1 giá trị
    /// </summary>
    public void UpdateTask(TypeTaskDaily typeTask)
    {
        CheckAndResetNewDay();
        var task = GetTaskProgress(typeTask);
        task.currentValue += 1;
        DataAccountPlayer.SavePlayerDailyTashkData();
        NotifyManager.RefreshAllNoti();
    }

    public void IncreaseTask(TypeTaskDaily typeTask) => UpdateTask(typeTask);

    public void AddTaskProgress(TypeTaskDaily typeTask, int delta)
    {
        CheckAndResetNewDay();
        var task = GetTaskProgress(typeTask);
        task.currentValue += delta;
        DataAccountPlayer.SavePlayerDailyTashkData();
        NotifyManager.RefreshAllNoti();
    }

    /// <summary>
    /// Kiểm tra mỗi khi call sẽ compare giá trị hiện tại của task và valueTask nếu >= nhau thì trả về true, < trả về false
    /// </summary>
    public bool CheckTask(TypeTaskDaily typeTask)
    {
        CheckAndResetNewDay();

        var dataDailyTask = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.DataDailyTaskController()
            : Resources.Load<DataDailyTaskController>("Data/DataDailyTask");

        if (dataDailyTask != null)
        {
            var item = dataDailyTask.itemDailyTask(typeTask);
            if (item != null)
            {
                return GetCurrentValue(typeTask) >= item.valueTask;
            }
        }
        return false;
    }

    public bool CheckTask(TypeTaskDaily typeTask, int valueTask)
    {
        CheckAndResetNewDay();
        return GetCurrentValue(typeTask) >= valueTask;
    }

    public bool IsTaskCompleted(TypeTaskDaily typeTask) => CheckTask(typeTask);

    public bool CanClaimTask(TypeTaskDaily typeTask)
    {
        return CheckTask(typeTask) && !IsTaskClaimed(typeTask);
    }
}
