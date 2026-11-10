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
        _bundleRewards       = reward;
        bundleId             = reward[0].idBundle;
        bundleNameText.text  = reward[0].name;
        bundlePriceText.text = reward[0].price.ToString();
        _isProduct           = isProduct;
        var icon = LoadResourceController.Instance.LoadItemIcon(reward[0].iconBundle);
        if (icon != null)
        {
            bundleIcon.sprite = icon;
        }
        else
        {
            GameDebug.LogWarning($"[BundleBase] Không tìm thấy icon bundle: {reward[0].iconBundle}");
        }
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

        var listReward = new List<PopupReward>(_bundleRewards.Count);
        for (int i = 0; i < _bundleRewards.Count; i++)
        {
            var reward = _bundleRewards[i];
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
