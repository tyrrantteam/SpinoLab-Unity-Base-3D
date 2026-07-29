
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyCheckinController : PopupBaseController
{
    [SerializeField] private DailyCheckinElements[] dailyElement;
    [SerializeField] private Text timeCountTxt;

    private List<PopupRewardDailyCheckin> dailyCheckin = new List<PopupRewardDailyCheckin>();

    private Coroutine countdownCoroutine;

    protected override void Awake()
    {
        base.Awake();
        this.RegisterListener(EventID.ClaimDailyCheckin, (sender, param) => CountTime());
    }

    protected override void InitData()
    {
        base.InitData();
        dailyCheckin = LoadResourceController.Instance.DataRewardController().ContentContent.dailyCheckin;
        InitDailyElementData();
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        CountTime();
    }

    private void CountTime()
    {
        var time = DataAccountPlayer.PlayerDailyCheckinData.lastTimeCheckin;
        DateTime endTime = time.AddHours(24);
        TimeSpan remaining = endTime - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0)
        {
            timeCountTxt.gameObject.SetActive(false);
        }
        else
        {
            timeCountTxt.gameObject.SetActive(true);
            countdownCoroutine = StartCoroutine(CountdownCoroutine(endTime));
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
    }


    private void InitDailyElementData()
    {
        for (int i = 0; i < dailyElement.Length; i++)
        {

            List<PopupRewardDailyCheckin> dailyCheckinByDay = new List<PopupRewardDailyCheckin>();
            var itemsWithId3 = dailyCheckin.Where(obj => obj.day == i).ToList();
            dailyCheckinByDay.AddRange(itemsWithId3);
            dailyElement[i].InitData(dailyCheckinByDay);
        }
    }

    public void StartCountdownFrom(DateTime startTime)
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }

        DateTime endTime = startTime.AddHours(24);
        countdownCoroutine = StartCoroutine(CountdownCoroutine(endTime));
    }

    private IEnumerator CountdownCoroutine(DateTime targetTime)
    {
        while (true)
        {
            TimeSpan remaining = targetTime - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0)
            {
                timeCountTxt.text = "00:00:00";
                timeCountTxt.gameObject.SetActive(false);
                yield break;
            }

            timeCountTxt.text = "Next Reward in : " + string.Format("{0:D2}:{1:D2}:{2:D2}",
                remaining.Hours + remaining.Days * 24,
                remaining.Minutes,
                remaining.Seconds);

            yield return new WaitForSeconds(1f);
        }
    }
}

