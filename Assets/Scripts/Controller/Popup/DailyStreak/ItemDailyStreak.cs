using System;
using System.Collections;
using JinGroup.Controller.Feature;
using TMPro;
using UnityEngine;

public class ItemDailyStreak : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayTxt;
    [SerializeField] private ScaleEffectSquashAndStretch checkinStreak;
    [SerializeField] private GameObject vfxCheckin;

    [SerializeField] private GameObject gift;
    [SerializeField] private GameObject decorGift;

    public bool isGift { get; private set; }

    public void Setup(
        int day,
        bool isGiftDay,
        Color checkinColor,
        Color normalColor,
        bool alreadyCheckedIn,
        bool isTodayCheckin)
    {
        isGift = isGiftDay;
        dayTxt.text = day.ToString();

        SetupGift(isGiftDay);

        if (isGiftDay)
        {
            dayTxt.color = isTodayCheckin ? checkinColor : normalColor;
            return;
        }

        if (alreadyCheckedIn)
        {
            dayTxt.color = normalColor;
            ShowCheckinImmediate();
            return;
        }

        if (isTodayCheckin)
        {
            dayTxt.color = checkinColor;
            checkinStreak.gameObject.SetActive(false);
            return;
        }

        dayTxt.color = normalColor;
        checkinStreak.gameObject.SetActive(false);
    }

    public IEnumerator PlayCheckinRoutine(int castDelaySeconds, Action onComplete)
    {
        if (isGift)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (castDelaySeconds > 0)
            yield return new WaitForSeconds(castDelaySeconds);

        checkinStreak.gameObject.SetActive(true);
        checkinStreak.Play();

        if (vfxCheckin != null)
            vfxCheckin.SetActive(true);

        yield return new WaitForSeconds(checkinStreak.duration);
        onComplete?.Invoke();
    }

    private void SetupGift(bool isGiftDay)
    {
        if (checkinStreak != null)
            checkinStreak.gameObject.SetActive(false);

        if (gift != null)
            gift.SetActive(isGiftDay);

        if (decorGift != null)
            decorGift.SetActive(isGiftDay);
    }

    private void ShowCheckinImmediate()
    {
        if (checkinStreak == null)
            return;

        checkinStreak.gameObject.SetActive(true);
        checkinStreak.Stop();
    }
}
