using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Base.Core.Debug;
using TMPro;

public abstract class BundleBase : MonoBehaviour
{
    public Image bundleIcon;

    public TMP_Text bundleNameText;
    public TMP_Text bundlePriceText;

    protected string             bundleId;
    protected List<BundleReward> _bundleRewards;

    protected Button btnBuy;
    private   bool   _isProduct;

    protected virtual void Awake()
    {
        btnBuy = GetComponent<Button>();

        if (btnBuy != null)
        {
            btnBuy.onClick.AddListener(OnStartPurchase);
        }
    }

    public virtual void LoadBundle(List<BundleReward> reward, int index, bool isProduct)
    {
        _bundleRewards = reward;
        var data = reward[0];
        bundleId             = data.idBundle;
        bundleNameText.text  = data.name;
        bundlePriceText.text = data.price.ToString();
        _isProduct           = isProduct;
        bundleIcon.sprite    = data.imgIconBundle;
    }

    protected virtual void OnStartPurchase()
    {
        if (_isProduct)
        {
            // todo: inseart inapp purchase
            ShowClaimRewardPopup();
        }
        else
        {
            ShowClaimRewardPopup();
        }
    }

    protected abstract void OnGrantProducts();

    protected void ShowClaimRewardPopup()
    {
        if (_bundleRewards == null || _bundleRewards.Count == 0)
        {
            return;
        }

        var listReward = RewardHelper.ToPopupRewards(_bundleRewards);

        var popup = PopupManager.Instance != null ? PopupManager.Instance.ModuleShowReward() : null;
        if (popup != null)
        {
            popup.OpenPopup(listReward);
            OnGrantProducts();
        }
        else
        {
            OnGrantProducts();
        }
    }

    protected virtual void GrantAllRewards()
    {
        this.PostEvent(EventID.OnPurchaseIAP);
        RewardHelper.GrantAll(_bundleRewards);
    }
}