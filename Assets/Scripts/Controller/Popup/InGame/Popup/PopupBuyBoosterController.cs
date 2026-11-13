using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupBuyBoosterController : PopupBaseController
{
    [SerializeField] private Button claimAdsBtn;
    [SerializeField] private TextMeshProUGUI valueGetAds;
    [SerializeField] private Button claimGoldBtn;
    [SerializeField] private Image boosterImgPreview;
    [SerializeField] private TextMeshProUGUI valueGetGold;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI nameBoosterTxt;
    [SerializeField] private TextMeshProUGUI boosterDesTxt;

    private BoosterType currentBoosterType;
    private ProcessBoosterData boosterData;
    private int mutipleValueGold = 1;
    private int mutipleValueAds = 1;
    private int price = 0;

    private DataBoosterController boosterController;
    private DataAdsController dataAds;
    private GameConfig gameConfig;
    private bool isClaimed = false;

    protected override void InitData()
    {
        base.InitData();
        EnsureControllers();
        UpdateVisibility();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        isClaimed = false;
        if (claimAdsBtn != null) claimAdsBtn.interactable = true;
        if (claimGoldBtn != null) claimGoldBtn.interactable = true;

        if (currentBoosterType != BoosterType.None)
        {
            UpdateUI();
        }
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();

        if (claimAdsBtn != null)
            claimAdsBtn.onClick.AddListener(OnClickAdsBtn);

        if (claimGoldBtn != null)
            claimGoldBtn.onClick.AddListener(OnClickGoldBtn);
    }

    private void EnsureControllers()
    {
        if (boosterController == null && !LoadResourceController.IsNull)
            boosterController = LoadResourceController.Instance.DataBoosterController();

        if (dataAds == null && !LoadResourceController.IsNull)
            dataAds = LoadResourceController.Instance.DataAdsController();

        if (gameConfig == null && !LoadResourceController.IsNull)
            gameConfig = LoadResourceController.Instance.GameConfig();
    }

    public void Setup(BoosterType type)
    {
        currentBoosterType = type;
        isClaimed = false;

        EnsureControllers();

        if (boosterController != null)
        {
            boosterData = boosterController.GetDataBoosterByType(type);
        }

        mutipleValueGold = boosterData.mutipleValueGold > 0 ? boosterData.mutipleValueGold : 1;
        mutipleValueAds = boosterData.mutipleValueAds > 0 ? boosterData.mutipleValueAds : 1;
        price = boosterData.price;
        boosterDesTxt.text = boosterData.description;
        nameBoosterTxt.text = boosterData.nameBooster;

        UpdateUI();
        UpdateVisibility();
    }

    public void InitData(BoosterType type)
    {
        Setup(type);
    }

    private void UpdateVisibility()
    {
        if (claimAdsBtn != null && dataAds != null)
            claimAdsBtn.gameObject.SetActive(dataAds.UsingRewardAds);

        if (claimGoldBtn != null && gameConfig != null)
            claimGoldBtn.gameObject.SetActive(gameConfig.usingResourceInGame);
    }

    private void UpdateUI()
    {
        // priceText = price booster tương ứng trong data nhân với mutipleValueGold
        int totalPrice = price * mutipleValueGold;
        if (priceText != null)
            priceText.text = totalPrice.ToString();

        // valueGetAds = mutipleValueAds
        if (valueGetAds != null)
            valueGetAds.text = mutipleValueAds.ToString();

        // claimGoldBtn = mutipleValueGold (valueGetGold)
        if (valueGetGold != null)
            valueGetGold.text = mutipleValueGold.ToString();

        // boosterImgPreview = imgMechanic
        if (boosterImgPreview != null && boosterData.imgMechanic != null)
            boosterImgPreview.sprite = boosterData.imgMechanic;
    }

    private void OnClickAdsBtn()
    {
        UseBoosterFromAds(currentBoosterType);
    }

    private void OnClickGoldBtn()
    {
        UseBoosterFromGold(currentBoosterType);
    }

    public void UseBoosterFromAds(BoosterType boosterType)
    {
        if (isClaimed) return;
        isClaimed = true;

        if (claimAdsBtn != null) claimAdsBtn.interactable = false;
        if (claimGoldBtn != null) claimGoldBtn.interactable = false;

        if (!AnalyticManager.IsNull)
            AnalyticManager.Instance.BoosterClaim(boosterType);

        int rewardCount = mutipleValueAds > 0 ? mutipleValueAds : 1;

        if (!GameController.IsNull)
        {
            int visualCount = Mathf.Clamp(rewardCount, 1, 5);
            GameController.Instance.PlaySpawnIconBoosterItem(boosterType, () =>
            {
                if (!BoosterManager.IsNull)
                    BoosterManager.Instance.ApplyBoosterReward(boosterType, rewardCount);
                else
                    DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, rewardCount);
            }, visualCount);
        }
        else
        {
            if (!BoosterManager.IsNull)
                BoosterManager.Instance.ApplyBoosterReward(boosterType, rewardCount);
            else
                DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, rewardCount);
        }

        GameDebug.Log($"[PopupBuyBoosterController] UseBoosterFromAds: Claimed {rewardCount} booster {boosterType}");
        OnClosePopup();
    }

    public void UseBoosterFromGold(BoosterType boosterType)
    {
        if (isClaimed) return;

        int rewardCount = mutipleValueGold > 0 ? mutipleValueGold : 1;
        int totalPrice = price * rewardCount;
        int playerGold = DataAccountPlayer.PlayerResourceData.gold;

        if (playerGold >= totalPrice)
        {
            isClaimed = true;

            if (claimAdsBtn != null) claimAdsBtn.interactable = false;
            if (claimGoldBtn != null) claimGoldBtn.interactable = false;

            DataAccountPlayer.PlayerResourceData.ChangeGoldValue(-totalPrice);

            if (!AnalyticManager.IsNull)
                AnalyticManager.Instance.BoosterClaim(boosterType);

            if (!GameController.IsNull)
            {
                int visualCount = Mathf.Clamp(rewardCount, 1, 5);
                GameController.Instance.PlaySpawnIconBoosterItem(boosterType, () =>
                {
                    if (!BoosterManager.IsNull)
                        BoosterManager.Instance.ApplyBoosterReward(boosterType, rewardCount);
                    else
                        DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, rewardCount);
                }, visualCount);
            }
            else
            {
                if (!BoosterManager.IsNull)
                    BoosterManager.Instance.ApplyBoosterReward(boosterType, rewardCount);
                else
                    DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, rewardCount);
            }

            GameDebug.Log($"[PopupBuyBoosterController] UseBoosterFromGold: Bought {rewardCount} booster {boosterType} with {totalPrice} gold");
            OnClosePopup();
        }
        else
        {
            if (!ToastManager.IsNull)
            {
                ToastManager.Instance.Show("Not enough gold!");
            }
            GameDebug.Log($"[PopupBuyBoosterController] Not enough gold: needed {totalPrice}, current {playerGold}");
        }
    }
}
