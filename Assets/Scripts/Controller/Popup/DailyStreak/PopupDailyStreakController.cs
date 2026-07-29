using System.Collections;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Common.UIBaseController;
using UnityEngine;
using UnityEngine.UI;

public class PopupDailyStreakController : PopupBaseController
{
    private const float ShowItemsDelay = 0.5f;

    [SerializeField] private ItemDailyStreak itemDailyStreak;
    [SerializeField] private Transform parentItem;
    [SerializeField] private GameObject decorArt;
    [SerializeField] private Button claimBtn;

    private readonly List<ItemDailyStreak> _spawnedItems = new();
    private DataDailyStreak _config;
    private int _dayToCheckIn;
    private bool _isClaiming;
    private Coroutine _showRoutine;

    protected override void ListenerButton()
    {
        base.ListenerButton();
        if (claimBtn != null)
            claimBtn.onClick.AddListener(OnClaimFinalReward);
    }

    public void Setup(int dayToCheckIn, DataDailyStreak config)
    {
        _dayToCheckIn = dayToCheckIn;
        _config = config;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (_config == null)
            return;

        _isClaiming = false;
        _showRoutine = StartCoroutine(ShowRoutine());
    }

    protected override void OnDisable()
    {
        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        _config = null;
        _isClaiming = false;

        if (claimBtn != null)
            claimBtn.interactable = true;

        DailyStreakService.NotifyPopupClosed();
        base.OnDisable();
    }

    private IEnumerator ShowRoutine()
    {
        if (_config == null || itemDailyStreak == null || parentItem == null)
            yield break;

        if (claimBtn != null)
            claimBtn.gameObject.SetActive(false);

        if (decorArt != null)
            decorArt.SetActive(false);

        ClearSpawnedItems();
        BuildItems(active: false);

        yield return new WaitForSeconds(ShowItemsDelay);

        if (decorArt != null)
            decorArt.SetActive(true);
        if (parentItem != null)
            parentItem.gameObject.SetActive(true);

        for (int i = 0; i < _spawnedItems.Count; i++)
            _spawnedItems[i].gameObject.SetActive(true);

        bool isFinalDay = _dayToCheckIn >= _config.streakDayCount;
        ItemDailyStreak todayItem = _spawnedItems[_dayToCheckIn - 1];

        if (isFinalDay)
        {
            if (claimBtn != null)
                claimBtn.gameObject.SetActive(true);
            yield break;
        }

        yield return todayItem.PlayCheckinRoutine(_config.timeCastCheckinItem, null);
        DataAccountPlayer.PlayerStreakData.CompleteDayCheckin(_dayToCheckIn);

        yield return new WaitForSeconds(_config.timeClose);
        OnClosePopup();
    }

    private void BuildItems(bool active)
    {
        int totalDays = _config.streakDayCount;

        for (int day = 1; day <= totalDays; day++)
        {
            bool isGift = day == totalDays;
            bool alreadyCheckedIn = day < _dayToCheckIn;
            bool isTodayCheckin = day == _dayToCheckIn;

            var item = Instantiate(itemDailyStreak, parentItem);
            item.gameObject.SetActive(active);
            item.Setup(
                day,
                isGift,
                _config.colorCheckin,
                _config.colorNormal,
                alreadyCheckedIn,
                isTodayCheckin);

            _spawnedItems.Add(item);
        }
    }

    private void ClearSpawnedItems()
    {
        for (int i = 0; i < _spawnedItems.Count; i++)
        {
            if (_spawnedItems[i] != null)
                Destroy(_spawnedItems[i].gameObject);
        }

        _spawnedItems.Clear();
    }

    private void OnClaimFinalReward()
    {
        if (_isClaiming || _config == null)
            return;

        _isClaiming = true;

        if (claimBtn != null)
            claimBtn.interactable = false;

        var config = _config;
        int day = _dayToCheckIn;

        DataAccountPlayer.PlayerStreakData.CompleteDayCheckin(day);
        DataAccountPlayer.PlayerStreakData.MarkFinalRewardClaimed();

        SpawnRewards(config);
        OnClosePopup();
    }

    private void SpawnRewards(DataDailyStreak config)
    {
        var rewards = config.GetFinalRewardItems();
        if (rewards == null || rewards.Count == 0)
            return;

        for (int i = 0; i < rewards.Count; i++)
        {
            var reward = rewards[i];
            if (reward.boosterType == BoosterType.None || reward.boosterCount <= 0)
                continue;

            if (!BoosterManager.IsBoosterUnlockedAtCurrentLevel(reward.boosterType))
                continue;

            int visualCount = Mathf.Clamp(reward.boosterCount, 1, 5);
            var boosterType = reward.boosterType;
            var boosterCount = reward.boosterCount;

            GameController.Instance.PlaySpawnIconBoosterItem(
                boosterType,
                () =>
                {
                    if (!BoosterManager.IsNull)
                        BoosterManager.Instance.ApplyBoosterReward(boosterType, boosterCount);
                    else
                        DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, boosterCount);
                },
                visualCount);
        }
    }
}
