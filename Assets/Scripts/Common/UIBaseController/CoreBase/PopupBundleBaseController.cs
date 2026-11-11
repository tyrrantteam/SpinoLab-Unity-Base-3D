using Base.Core.Debug;
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

    protected bool HasBundleData => bundleData != null && bundleData.Count > 0;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetPurchaseInteractable(HasBundleData);
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        if (purchaseBtn != null)
            purchaseBtn.onClick.AddListener(OnProcessPurchase);
    }

    protected override void InitData()
    {
        base.InitData();
        if (!HasBundleData)
        {
            GameDebug.LogWarning($"[{GetType().Name}] Không có dữ liệu bundle!");
            SetPurchaseInteractable(false);
            return;
        }

        InitDataForReward(bundleData);
        idBundle = bundleData[0].idBundle;
        UpdatePrice(bundleData[0].price.ToString());
    }

    /// <summary>
    /// Entry point of the Buy button. Disables the button so the bundle can't be bought twice.
    /// </summary>
    public virtual void OnProcessPurchase()
    {
        if (!HasBundleData)
            return;

        SetPurchaseInteractable(false);

        // TODO(IAP): request the store purchase for idBundle here and call
        // OnPurchaseSucceeded() from the store's success callback instead.
        OnPurchaseSucceeded();
    }

    /// <summary>Runs after the purchase is confirmed. Subclasses add their own effects here.</summary>
    protected virtual void OnPurchaseSucceeded()
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
        if (!HasBundleData)
        {
            return;
        }

        var listReward = RewardHelper.ToPopupRewards(bundleData);

        var popup = PopupManager.Instance != null ? PopupManager.Instance.ModuleShowReward() : null;
        if (popup != null)
        {
            popup.OpenPopup(listReward, GrantAllRewards);
        }
        else
        {
            GrantAllRewards();
        }

        // Close even if the purchase happened during the open close-block.
        _canClose = true;
        OnClosePopup();
    }

    public void UpdatePrice(string price)
    {
        if (priceTxt != null)
            priceTxt.text = price;
    }

    public void InitDataForReward(List<BundleReward> rewardList)
    {
        RewardHelper.ShowRewards(rewards, rewardList);
    }

    private void SetPurchaseInteractable(bool value)
    {
        if (purchaseBtn != null)
            purchaseBtn.interactable = value;
    }
}
