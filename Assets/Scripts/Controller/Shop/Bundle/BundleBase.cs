using System;
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

    private Button btnBuy;

    private void Awake()
    {
        btnBuy = GetComponent<Button>();

        if (btnBuy != null)
        {
            btnBuy.onClick.AddListener(OnStartPurchase);
        }
    }

    public virtual void LoadBundle(List<BundleReward> reward, int index)
    {
        _bundleRewards       = reward;
        bundleId             = reward[0].idBundle;
        bundleNameText.text  = reward[0].name;
        bundlePriceText.text = reward[0].price.ToString();

        var icon = LoadResourceController.Instance.LoadItemIcon(reward[0].iconBundle);
        if (icon != null)
        {
            bundleIcon.sprite = icon;
        }
        else
        {
            GameDebug.LogError($"[BundleBase] Không tìm thấy icon bundle: {reward[0].iconBundle}");
        }
    }

    private void OnStartPurchase()
    {
        ShowClaimRewardPopup();
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
        if (_bundleRewards == null || _bundleRewards.Count == 0)
        {
            GameDebug.LogWarning("[BundleBase] Không có dữ liệu phần thưởng!");
            return;
        }

        foreach (var reward in _bundleRewards)
        {
            GameDebug.Log($"- Item ID: {reward.id} | Cần cộng: {reward.value} [{reward.typeResources}]");
            if (!Enum.TryParse<TypeResources>(reward.typeResources, true, out var type))
            {
                continue;
            }

            var data = DataAccount.DataAccountPlayer.PlayerResourceData;
            switch (type)
            {
                case TypeResources.gold:
                    data.ChangeGoldValue(reward.value);
                    break;

                case TypeResources.diamond:
                    data.ChangeDiamondValue(reward.value);
                    break;

                case TypeResources.heart:
                    // TODO: Sẽ implement lại sau
                    // data.AddFreeHeartHours(reward.value);
                    break;

                case TypeResources.booster1:
                    data.ChangeBoosterCount(BoosterType.booster1, reward.value);
                    break;

                case TypeResources.booster2:
                    data.ChangeBoosterCount(BoosterType.booster2, reward.value);
                    break;

                case TypeResources.booster3:
                    data.ChangeBoosterCount(BoosterType.booster3, reward.value);
                    break;
            }
        }
    }
}
