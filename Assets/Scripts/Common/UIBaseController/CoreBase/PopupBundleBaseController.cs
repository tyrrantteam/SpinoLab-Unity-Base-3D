using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupBundleBaseController : PopupBaseController
{
    public                   string                       idBundle;
    public                   List<BundleReward>           bundleData;
    public                   List<ModuleRewardController> rewards;
    [SerializeField] private Button                       purchaseBtn;
    [SerializeField] private TextMeshProUGUI              priceTxt;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        purchaseBtn.onClick.AddListener(OnProcessPurchase);
    }

    protected override void InitData()
    {
        base.InitData();
        InitDataForReward(bundleData);
        var price = bundleData[0].price;
        idBundle = bundleData[0].idBundle;
        UpdatePrice(price.ToString());
    }

    public virtual void OnProcessPurchase()
    {
        ShowClaimRewardPopup();
    }

    protected virtual void GrantAllRewards()
    {
        this.PostEvent(EventID.OnPurchaseIAP);
        RewardHelper.GrantAll(bundleData);
    }

    protected void ShowClaimRewardPopup()
    {
        if (bundleData == null || bundleData.Count == 0)
        {
            return;
        }

        var listReward = new List<PopupReward>(bundleData.Count);
        for (int i = 0; i < bundleData.Count; i++)
        {
            var reward = bundleData[i];
            listReward.Add(new PopupReward
            {
                id            = reward.id,
                value         = reward.value,
                typeResources = reward.typeResources
            });
        }

        var popup = PopupManager.Instance != null ? PopupManager.Instance.ModuleShowReward() : null;
        if (popup != null)
        {
            popup.OpenPopup(listReward, () =>
            {
                GrantAllRewards();
            });
        }
        else
        {
            GrantAllRewards();
        }
        OnClosePopup();
    }

    public void UpdatePrice(string price)
    {
        priceTxt.text = price;
    }

    public void InitDataForReward(List<BundleReward> rewardList)
    {
        RewardHelper.ShowRewards(rewards, rewardList);
    }
}
