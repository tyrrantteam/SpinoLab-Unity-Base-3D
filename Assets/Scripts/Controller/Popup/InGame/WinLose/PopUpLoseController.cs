using Base.Core;
using Controller.Shop;
using DataAccount;
using JinGroup.Common.UIBaseController;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class PopUpLoseController : PopupBaseController
{
    //[SerializeField] private Button homeBtn;
    [SerializeField] private Button             tryAgainBtn;
    [SerializeField] private Button             homeBtn;
    [SerializeField] private GameObject         headerDecor;
    [SerializeField] private GameConfig         gameConfig;
    [SerializeField] private ShopManagerWinLose shopManagerWinLose;

    protected override void Awake()
    {
        base.Awake();
        shopManagerWinLose.InitBundles(BundleType.Bundle);
        this.RegisterListener(EventID.OnPurchaseIAP, OnPurchaseIAP);
    }



    protected override void OnEnable()
    {
        Invoke("ActiveHeaderDecor", 0.75f);
        Invoke("ActiveBtnNextDecor", 0.75f);
        CheckHomeButtonStatus();
        CheckBtnTryStatus();
       
    }
    private void OnPurchaseIAP(Component arg1, object arg2)
    {
        tryAgainBtn.gameObject.SetActive(true);
    }

    private void ActiveHeaderDecor()
    {
        
        headerDecor.SetActive(true);
    }

    private void ActiveBtnNextDecor()
    {
        tryAgainBtn.gameObject.SetActive(true);
    }

    private void CheckHomeButtonStatus()
    {
        if (gameConfig.usingMetaSys)
        {
            var resourceData = DataAccountPlayer.PlayerResourceData;
            var dataLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
            homeBtn.gameObject.SetActive(gameConfig.levelUnlockMetaSystem < dataLv);
            return;
        }
        homeBtn.gameObject.SetActive(false);
    }

    private void CheckBtnTryStatus()
    {
        var data = DataAccountPlayer.PlayerResourceData;
        if (!gameConfig.usingHeart)
        {
            tryAgainBtn.gameObject.SetActive(true);
            return;
        }

        if (data.heart > 0 || data.isInfiniteHeartActive)
        {
            tryAgainBtn.gameObject.SetActive(true);
        }
        else
        {
            tryAgainBtn.gameObject.SetActive(false);
        }
    }
    
    protected override void ListenerButton()
    {
        homeBtn.onClick.AddListener(OnClickHome);
        tryAgainBtn.onClick.AddListener(OnTryAgain);
    }

    private void OnClickHome()
    {
        GameManager.Instance.LoadScene(SceneName.HomeScene);
        OnClosePopup();
    }

    private void OnTryAgain()
    {
        //GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        AnalyticManager.Instance.LogEventLevelTryAgain();
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        OnClosePopup();
    }
}