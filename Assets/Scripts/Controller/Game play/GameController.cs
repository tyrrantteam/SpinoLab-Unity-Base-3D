using Base.Core.Debug;
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
    [SerializeField] private BoosterManager boosterManager;
    [SerializeField] private SpawnCircleEffectUI SpawnCircleEffectUI;
    private MapManager _mapManager;
    private DataProcessMechanic dataTut;
    private GameConfig _gameConfig;
    private DataLoopLevelController _dataLoopLevelController;
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

        if (_dataLoopLevelController == null)
        {
            _dataLoopLevelController = LoadResourceController.Instance.DataLoopLevelController();
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
        boosterManager.gameObject.SetActive(_gameConfig.usingBooster);
        var sampleLv = LoadResourceController.Instance.LevelGame(_currentLevel);
        //_mapManager = Instantiate(sampleLv, sampleLv.transform.position, sampleLv.transform.rotation);
        AnalyticManager.instance.LogEventLevelStart();
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


        if (levelNumber >= _dataLoopLevelController.levelStartRandom)
        {
            var newID = _dataLoopLevelController.levelRandom(levelNumber);
            GameDebug.Log($"Level {levelNumber} is greater than or equal to levelStartRandom {_dataLoopLevelController.levelStartRandom}. New level ID: {newID}");
            playerPointProcessData.SetCurrentLevel(newID);
            playerPointProcessData.ChangeprogressConfigId();
        }
    }

    public void SetWinGame()
    {
        DataAccountPlayer.PlayerLuckySpinData.IncreasecurrentLevelProcess();
        AnalyticManager.instance.LogEventLevelComplete();
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
        AnalyticManager.instance.LogEventLevelFailed();
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
