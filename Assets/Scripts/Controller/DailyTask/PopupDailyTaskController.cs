using DataAccount;
using JinGroup.Base.LoadData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyTaskController : PopupBaseController
{
    [SerializeField] private DailyTaskElementController dailyTaskSample;
    [SerializeField] private Transform holderdailyTask;
    [SerializeField] private TextMeshProUGUI timeCountingReset;
    [SerializeField] private Button claimAllBtn;

    private DataDailyTaskController _dataDailyTask;
    private readonly List<DailyTaskElementController> _elements = new List<DailyTaskElementController>();
    private Coroutine _countdownRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (dailyTaskSample != null && dailyTaskSample.gameObject.scene.IsValid())
        {
            dailyTaskSample.gameObject.SetActive(false);
        }
    }

    protected override void InitData()
    {
        base.InitData();

        if (_dataDailyTask == null)
        {
            if (LoadResourceController.Instance != null)
                _dataDailyTask = LoadResourceController.Instance.DataDailyTaskController();
            if (_dataDailyTask == null)
                _dataDailyTask = Resources.Load<DataDailyTaskController>("Data/DataDailyTask");
        }

        DataAccountPlayer.PlayerDailyTashkData.CheckAndResetNewDay();
        SpawnOrUpdateTasks();
        UpdateClaimAllButtonState();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InitData();

        if (_countdownRoutine != null)
            StopCoroutine(_countdownRoutine);
        _countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_countdownRoutine != null)
        {
            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
        }
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        if (claimAllBtn != null)
        {
            claimAllBtn.onClick.RemoveAllListeners();
            claimAllBtn.onClick.AddListener(OnClickClaimAll);
        }
    }

    /// <summary>
    /// Sinh dailyTaskSample theo số lượng task element có trong DataDailyTaskController vào holderdailyTask
    /// </summary>
    private void SpawnOrUpdateTasks()
    {
        if (_dataDailyTask == null || _dataDailyTask.ContentContent?.listDailyTask == null)
            return;

        var listData = _dataDailyTask.ContentContent.listDailyTask;
        Transform parent = holderdailyTask != null ? holderdailyTask : transform;

        // Nếu đã có sẵn trong holderdailyTask từ trước (hoặc cache)
        if (_elements.Count == 0 && holderdailyTask != null)
        {
            var existing = holderdailyTask.GetComponentsInChildren<DailyTaskElementController>(true);
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != dailyTaskSample)
                    _elements.Add(existing[i]);
            }
        }

        for (int i = 0; i < listData.Count; i++)
        {
            DailyTaskElementController elem;
            if (i < _elements.Count)
            {
                elem = _elements[i];
            }
            else
            {
                if (dailyTaskSample != null)
                {
                    elem = Instantiate(dailyTaskSample, parent);
                    _elements.Add(elem);
                }
                else
                {
                    var prefab = Resources.Load<DailyTaskElementController>("Prefabs/UI/Popup/Meta/DailyTask/ElementTask");
                    if (prefab != null)
                    {
                        elem = Instantiate(prefab, parent);
                        _elements.Add(elem);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            elem.gameObject.SetActive(true);
            elem.InitData(listData[i], OnElementTaskClaimed);
        }

        for (int i = listData.Count; i < _elements.Count; i++)
        {
            _elements[i].gameObject.SetActive(false);
        }
    }

    private void OnElementTaskClaimed()
    {
        UpdateClaimAllButtonState();
        NotifyManager.RefreshAllNoti();
    }

    private void UpdateClaimAllButtonState()
    {
        if (claimAllBtn == null || _dataDailyTask == null || _dataDailyTask.ContentContent?.listDailyTask == null)
            return;

        bool hasClaimable = false;
        var listData = _dataDailyTask.ContentContent.listDailyTask;
        for (int i = 0; i < listData.Count; i++)
        {
            if (DataAccountPlayer.PlayerDailyTashkData.CanClaimTask(listData[i].typeTask))
            {
                hasClaimable = true;
                break;
            }
        }
        claimAllBtn.interactable = hasClaimable;
    }

    /// <summary>
    /// claimAllBtn claim được tất cả các reward ở DailyTaskElementController đã đạt được điều kiện claim
    /// </summary>
    private void OnClickClaimAll()
    {
        if (_dataDailyTask == null || _dataDailyTask.ContentContent?.listDailyTask == null)
            return;

        var listData = _dataDailyTask.ContentContent.listDailyTask;
        List<PopupReward> rewardsClaimed = new List<PopupReward>();

        for (int i = 0; i < listData.Count; i++)
        {
            var task = listData[i];
            if (DataAccountPlayer.PlayerDailyTashkData.CanClaimTask(task.typeTask))
            {
                DataAccountPlayer.PlayerDailyTashkData.SetTaskClaimed(task.typeTask);
                DailyTaskElementController.AddRewardToAccount(task);
                rewardsClaimed.Add(new PopupReward
                {
                    typeResources = task.typeResources,
                    value = task.value
                });
            }
        }

        if (rewardsClaimed.Count == 0) return;

        // Hiển thị phần thưởng qua popup
        if (PopupManager.Instance != null)
        {
            if (rewardsClaimed.Count == 1)
            {
                var single = PopupManager.Instance.ShowPopup<PopupShowRewardSingleUnitController>(p =>
                {
                    p.ShowReward(rewardsClaimed[0]);
                });
                if (single == null)
                {
                    single = PopupManager.Instance.ModuleShowRewardSingleUnit();
                    if (single != null) single.ShowReward(rewardsClaimed[0]);
                }
            }
            else
            {
                var multi = PopupManager.Instance.ModuleShowReward();
                if (multi != null)
                {
                    multi.OpenPopup(rewardsClaimed);
                }
            }
        }

        SpawnOrUpdateTasks();
        UpdateClaimAllButtonState();
        NotifyManager.RefreshAllNoti();
    }

    /// <summary>
    /// Đếm ngược 24 tiếng tính theo h local đến 00:00:00 hôm sau, lưu vào DataDailyTaskController và hiển thị "Reset After : 00:00:00"
    /// </summary>
    private IEnumerator CountdownRoutine()
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            DateTime tomorrowMidnight = now.Date.AddDays(1);
            TimeSpan remaining = tomorrowMidnight - now;

            if (_dataDailyTask != null)
            {
                _dataDailyTask.timeCountingReset = remaining;
                _dataDailyTask.resetTimeDisplay = string.Format("Reset After : {0:D2}:{1:D2}:{2:D2}",
                    (int)remaining.TotalHours,
                    remaining.Minutes,
                    remaining.Seconds);
            }

            if (timeCountingReset != null)
            {
                timeCountingReset.text = string.Format("Reset After : {0:D2}:{1:D2}:{2:D2}",
                    (int)remaining.TotalHours,
                    remaining.Minutes,
                    remaining.Seconds);
            }

            if (remaining.TotalSeconds <= 0)
            {
                DataAccountPlayer.PlayerDailyTashkData.ResetAllTasks();
                SpawnOrUpdateTasks();
                UpdateClaimAllButtonState();
                NotifyManager.RefreshAllNoti();
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
