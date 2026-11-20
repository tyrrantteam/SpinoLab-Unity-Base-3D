using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JinGroup.Controller.BoosterFlow;

public class BoosterManager : SingletonMono<BoosterManager>
{
    [SerializeField] private BoosterButtonController       BoosterButtonControllerPrefab;
    [SerializeField] private Transform                     parentBoosterBtn;
    [SerializeField] private List<BoosterButtonController> listBoosterBtn = new List<BoosterButtonController>();
    private                  DataBoosterController         dataBooster;
    private GameConfig _gameConfig;
    protected override void Awake()
    {
       base.Awake();
       this.RegisterListener(EventID.OnUnlockBooster, (sender, param) => UnlockBoosterTutorial((BoosterType)param));
    }

    private void Start()
    {
        InitBoosterButtons();
    }

    #region InitBoosterButtons

    public void InitBoosterButtons()
    {
        ClearBoosterButtons();

        if (dataBooster == null)
        {
            dataBooster = LoadResourceController.Instance.DataBoosterController();
        }

        if(_gameConfig == null)
        {
            _gameConfig = LoadResourceController.Instance.GameConfig();
        }

        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;

        for (int i = 0; i < dataBooster.ContentContent.listBoooster.Count; i++)
        {
            var boosterData = dataBooster.ContentContent.listBoooster[i];
            if (boosterData.boosterType == BoosterType.None)
                continue;

            var button = Instantiate(BoosterButtonControllerPrefab, parentBoosterBtn);
            button.Init(this, boosterData, currentLevel, _gameConfig);
            listBoosterBtn.Add(button);
        }

        if (parentBoosterBtn is RectTransform parentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }

    public void TryShowDeferredPopups()
    {
        if (dataBooster == null)
            dataBooster = LoadResourceController.Instance.DataBoosterController();

        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        CheckShowPopUpBooster(currentLevel);
        SpecialOfferAdsService.TryShowPopup();
    }


    private void CheckShowPopUpBooster(int currentLevel)
    {
        foreach (var data in dataBooster.ContentContent.listBoooster)
        {
            var typeBooster = data.boosterType;
            var countBooster = DataAccountPlayer.PlayerResourceData.GetBoosterCount(typeBooster);
            var conditionA = countBooster <= 0;
            var conditionB = data.level == currentLevel;
            if (conditionA && conditionB)
            {
                var popup = PopupManager.Instance.ShowPopup<PopupBoosterUnlockController>();
                popup.InitData(data);
                return;
            }
        }
    }


    private void ClearBoosterButtons()
    {
        for (int i = 0; i < listBoosterBtn.Count; i++)
        {
            if (listBoosterBtn[i] != null)
                Destroy(listBoosterBtn[i].gameObject);
        }

        listBoosterBtn.Clear();
    }

    public void RefreshBoosterButtons()
    {
        if (listBoosterBtn == null || listBoosterBtn.Count == 0)
        {
            return;
        }

        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        for (int i = 0; i < listBoosterBtn.Count; i++)
        {
            if (listBoosterBtn[i] != null)
            {
                listBoosterBtn[i].SetStatus(currentLevel);
            }
        }
    }

    #endregion

    #region UsingBooster

    public BoosterButtonController GetBoosterButton(BoosterType boosterType)
    {
        if (listBoosterBtn == null || listBoosterBtn.Count == 0)
        {
            InitBoosterButtons();
        }

        for (int i = 0; i < listBoosterBtn.Count; i++)
        {
            if (listBoosterBtn[i] != null && listBoosterBtn[i].BoosterType == boosterType)
                return listBoosterBtn[i];
        }

        return null;
    }

    public void ApplyBoosterReward(BoosterType type, int addedCount)
    {
        if (type == BoosterType.None || addedCount <= 0)
            return;

        DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(type, addedCount);

        var button = GetBoosterButton(type);
        if (button != null)
            button.PlayReceiveBoosterEffect(addedCount);
        else
            RefreshBoosterButtons();
    }

    public static bool IsBoosterUnlockedAtCurrentLevel(BoosterType type)
    {
        if (type == BoosterType.None)
            return false;

        if (DataAccountPlayer.PlayerPointProcessData.cheatUnlockAllBoosters)
            return true;

        if (LoadResourceController.IsNull)
            return false;

        var dataBooster = LoadResourceController.Instance.DataBoosterController();
        var data = dataBooster.GetDataBoosterByType(type);
        if (data.boosterType == BoosterType.None)
            return false;

        int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        return currentLevel >= data.level;
    }


    public bool IsAnyBoosterReceiving
    {
        get
        {
            if (listBoosterBtn == null) return false;
            for (int i = 0; i < listBoosterBtn.Count; i++)
            {
                if (listBoosterBtn[i] != null && listBoosterBtn[i].IsReceivingBooster)
                    return true;
            }
            return false;
        }
    }

    public void ResetAllReceivingBoosterState()
    {
        if (listBoosterBtn == null) return;
        for (int i = 0; i < listBoosterBtn.Count; i++)
        {
            if (listBoosterBtn[i] != null)
                listBoosterBtn[i].SetReceivingBooster(false);
        }
    }

    public void UseBooster(BoosterType boosterType, bool IsBoosterEmpty = false)
    {
        if (IsAnyBoosterReceiving)
            return;

        if (IsBoosterEmpty)
        {
            SpecialOfferAdsService.RecordEmptyBoosterAttempt(boosterType);
            var popup = PopupManager.Instance.ShowPopup<PopupBuyBoosterController>(p => p.Setup(boosterType));
            if (popup != null)
            {
                popup.Setup(boosterType);
            }
            return;
        }

        OnBoosterActivated(boosterType);
        UIGameController.instance.BottomBarSlideDown();
    }

    public void BoosterUsingComplete(BoosterType boosterType)
    {
        DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, -1);
        UIGameController.Instance.BottomBarSlideUp();
        RefreshBoosterButtons();
        var button = GetBoosterButton(boosterType);
        if (button != null)
        {
            int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
            button.SetStatus(currentLevel);
        }
    }

    private void UnlockBoosterTutorial(BoosterType boosterType)
    {
        RefreshBoosterButtons();
        var button = GetBoosterButton(boosterType);
        TutorialManager.Instance.handMoveTutorial.SetPositionOnCanvas(button.gameObject);
    }

    protected virtual void OnBoosterActivated(BoosterType boosterType)
    {
        AnalyticManager.instance.BoosterUsing(boosterType);
        DataAccountPlayer.PlayerDailyTashkData.UpdateTask(TypeTaskDaily.UseBooster);
        GameDebug.Log($"BoosterManager: Activated booster2 {boosterType}");
    }

    #endregion
}
