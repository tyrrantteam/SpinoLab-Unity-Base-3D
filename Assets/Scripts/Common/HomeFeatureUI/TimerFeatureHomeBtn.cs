using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimerFeatureHomeBtn : BaseFeatureHomeBtn
{
    [SerializeField] private GameObject timeObject;
    [SerializeField] private TMP_Text timeCountDownTxt;
    [SerializeField] private TMP_Text nameTxt;
     private Coroutine _countdownCo;
     /// <summary>
    /// Đếm ngược tới <paramref name="endTime"/>, hiện HH:MM:SS lên <see cref="timeCountDownTxt"/>.
    /// Hết giờ → hiện 00:00:00 rồi dừng.
    /// </summary>
    public void StartCountdown(DateTime endTime)
    {
        StopCountdown();

        if (timeCountDownTxt == null)
            return; 

        if (timeObject != null)
            timeObject.SetActive(true);
        timeCountDownTxt.gameObject.SetActive(true);
        _countdownCo = StartCoroutine(CoCountdown(endTime));
    }

    /// <summary>
    /// Đếm ngược trong <paramref name="duration"/> kể từ lúc gọi.
    /// </summary>
    public void StartCountdown(TimeSpan duration)
    {
        StartCountdown(DateTime.Now + duration);
    }

    /// <summary>
    /// Đếm ngược <paramref name="totalSeconds"/> giây kể từ lúc gọi.
    /// </summary>
    public void StartCountdown(float totalSeconds)
    {
        StartCountdown(TimeSpan.FromSeconds(Mathf.Max(0f, totalSeconds)));
    }

    public void StopCountdown()
    {
        if (_countdownCo != null)
        {
            StopCoroutine(_countdownCo);
            _countdownCo = null;
        }

        if (timeObject != null)
            timeObject.SetActive(false);
    }

    private IEnumerator CoCountdown(DateTime endTime)
    {
        while (true)
        {
            TimeSpan remaining = endTime - DateTime.Now;
            if (remaining.TotalSeconds <= 0)
            {
                timeCountDownTxt.text = "00:00:00";
                _countdownCo = null;
                yield break;
            }

            int totalHours = remaining.Days * 24 + remaining.Hours;
            timeCountDownTxt.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                totalHours, remaining.Minutes, remaining.Seconds);

            yield return new WaitForSecondsRealtime(1f);
        }
    }
}
