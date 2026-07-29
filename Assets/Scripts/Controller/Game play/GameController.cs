using Base.Core.Sound;
using DataAccount;
using DG.Tweening;
using JinGroup.Base.LoadData;
using JinGroup.Controller.Feature;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : SingletonMono<GameController>
{
    [SerializeField] private GameObject confetiRainWin;
    [SerializeField] private GameObject confetiStartWin;
    [SerializeField] private GameObject fireWorkEffect;
    [SerializeField] private UIGameController uIGameController;
    [SerializeField] private SpawnCircleEffectUI SpawnCircleEffectUI;
    private MapManager _mapManager;
    private DataProcessMechanic dataTut;
    private GameConfig _gameConfig;
    private int _currentLevel;
    protected override void Awake()
    {
        if (dataTut == null)
        {
            dataTut = LoadResourceController.Instance.LoadDataProcessMechanic();
        }

        if (_gameConfig == null)
        {
            _gameConfig = LoadResourceController.Instance.GameConfig();
        }
    }
    void Start()
    {
        InitData();
    }

    [Button("TEST WIN", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void TestWin()
    {
        SetWinGame();
    }

    [Button("TEST LOSE", ButtonSizes.Large), GUIColor(1, 0, 0)]
    private void TestLose()
    {
        SetLoseGame();
    }

    private void InitData()
    {
        var playerPointProcessData = DataAccountPlayer.PlayerPointProcessData;
        var levelNumber = playerPointProcessData.currentlevelShowScreen;
        _currentLevel = playerPointProcessData.currentlevel;
        uIGameController.SetLevelText(levelNumber);

        var sampleLv = LoadResourceController.Instance.LevelGame(_currentLevel);
        //_mapManager = Instantiate(sampleLv, sampleLv.transform.position, sampleLv.transform.rotation);
        AnlyticManager.instance.LogEventLevelStart();
        StartCoroutine(RunGameplayEntryPopupFlow());
    }

    private IEnumerator RunGameplayEntryPopupFlow()
    {
        yield return null;

        const int maxWaitFrames = 60;
        for (int i = 0; i < maxWaitFrames && PopupManager.IsNull; i++)
            yield return null;

        if (PopupManager.IsNull)
            yield break;

        bool streakShown = DailyStreakService.TryShowPopup();
        if (streakShown)
            yield return new WaitUntil(() => !DailyStreakService.IsPopupOpen);

        TryShowMechanicUnlockPopup();

        if (!TutorialManager.IsNull)
            TutorialManager.Instance.TryShowDeferredTutorial();

        if (!BoosterManager.IsNull)
            BoosterManager.Instance.TryShowDeferredPopups();
    }
    #region Onboarding
    public void TryShowMechanicUnlockPopup()
    {
        if (dataTut == null)
            dataTut = LoadResourceController.Instance.DataProcessMechanic();

        if (dataTut == null || PopupManager.IsNull)
            return;

        var playerData = DataAccountPlayer.PlayerPointProcessData;
        int level = playerData.currentlevelShowScreen;

        if (!dataTut.TryGetUnshownMechanicAtLevel(level, playerData.shownMechanicUnlockIndices, out var mechanicData, out int listIndex))
            return;

        var popup = PopupManager.Instance.ShowPopup<PopupProcessUnlockMechanicController>();
        if (popup == null)
            return;

        popup.Setup(mechanicData, listIndex);
    }
    #endregion

    #region Resault

    public void CheckLevel()
    {
        var playerPointProcessData = DataAccountPlayer.PlayerPointProcessData;
        playerPointProcessData.WinLevel();
        DailyStreakService.OnLevelWon();
        var levelNumber = playerPointProcessData.currentlevelShowScreen;
        if (levelNumber >= _gameConfig.levelRandom)
        {
            int id = -1;
            int attempts = 0;
            int min = _gameConfig.levelMin;
            int max = _gameConfig.levelRandom; // exclusive trong Random.Range cho int
                          // nếu tất cả đều trùng thì sẽ clear
            if (playerPointProcessData.listLvLoop.Count >= (max - min))
            {
                playerPointProcessData.listLvLoop.Clear();
            }

            // random đến khi nào ra số chưa trùng
            do
            {
                id = Random.Range(min, max);
                attempts++;
            } while (DataAccountPlayer.PlayerPointProcessData.listLvLoop.Contains(id) && attempts < 1000);

            // add vào list
            playerPointProcessData.listLvLoop.Add(id);
            playerPointProcessData.SetCurrentLevel(id);
            playerPointProcessData.ChangeprogressConfigId();

        }
    }

    public void SetWinGame()
    {
        AnlyticManager.instance.LogEventLevelComplete();
        HapticManager.Instance.PlayHapticSuccess();
        confetiStartWin.SetActive(true);

        DOVirtual.DelayedCall(1f, () =>
        {
            confetiStartWin.SetActive(false);
            confetiRainWin.SetActive(true);
            SoundManager.Instance.PlaySound(SoundType.LevelCompleted);
            PopupManager.Instance.ShowPopup<PopUpWinController>();
        });
       
    }

    public void SetLoseGame()
    {
        AnlyticManager.instance.LogEventLevelFailed();
        HapticManager.Instance.PlayHapticFailure();
        SoundManager.Instance.PlaySound(SoundType.LevelFail);
        PopupManager.Instance.ShowPopup<PopUpLoseController>();
      
    }

    #endregion

    #region Effect
    public void PlaySpawnIconBoosterItem(BoosterType type, Action onComplete = null, int spawnCount = 1)
    {
        SpawnCircleEffectUI.OnSpawnBoosterUI(type, onComplete, spawnCount);
    }
    #endregion
}
