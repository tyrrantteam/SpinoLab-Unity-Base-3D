using System;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Base.LoadData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupGetMoreHeartController : PopupBaseController
{
    [SerializeField] private Button claimAds;
    [SerializeField] private Button claimGold;
    [SerializeField] private TextMeshProUGUI priceHeartTxt;
    [SerializeField] private TextMeshProUGUI valueHeartTxt;

    private DataHeartController _dataHeartController;
    private bool _isProcessing;

    protected override void Awake()
    {
        // Cho phép popup show đè lên popup khác, khi đóng chỉ tắt chính nó
        keepOtherPopupsOnClose = true;
        base.Awake();
    }

    protected override void OnEnable()
    {
        // Đảm bảo popup luôn nằm ở đỉnh (last sibling) đè lên tất cả popup khác
        transform.SetAsLastSibling();
        base.OnEnable();

        _isProcessing = false;
        if (claimAds != null) claimAds.interactable = true;
        if (claimGold != null) claimGold.interactable = true;

        UpdateUI();
    }

    protected override void InitData()
    {
        base.InitData();
        EnsureData();
        UpdateUI();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();

        if (claimAds != null)
        {
            claimAds.onClick.RemoveAllListeners();
            claimAds.onClick.AddListener(OnClickClaimAds);
        }

        if (claimGold != null)
        {
            claimGold.onClick.RemoveAllListeners();
            claimGold.onClick.AddListener(OnClickClaimGold);
        }
    }

    private void EnsureData()
    {
        if (_dataHeartController == null)
        {
            if (LoadResourceController.Instance != null)
            {
                _dataHeartController = LoadResourceController.Instance.DataHeartController();
            }

            if (_dataHeartController == null)
            {
                _dataHeartController = Resources.Load<DataHeartController>("Data/DataHeart");
            }
        }
    }

    /// <summary>
    /// Hiển thị giá priceHeart và số lượng valueHeart từ DataHeartController
    /// </summary>
    public void UpdateUI()
    {
        EnsureData();

        if (_dataHeartController != null)
        {
            // priceHeartTxt = PriceHeart trong data DataHeartController
            if (priceHeartTxt != null)
            {
                priceHeartTxt.text = _dataHeartController.PriceHeart.ToString();
            }

            // valueHeartTxt = valueHeart
            if (valueHeartTxt != null)
            {
                valueHeartTxt.text = _dataHeartController.ValueHeart.ToString();
            }
        }
    }

    private void OnClickClaimAds()
    {
        if (_isProcessing) return;

        if (!AdsManager.IsNull)
        {
            AdsManager.Instance.ShowRewarded(() =>
            {
                ClaimFromAds();
            });
#if UNITY_EDITOR
            // Fallback khi test trong Unity Editor nếu callback quảng cáo chưa tích hợp
            if (DataAccountPlayer.PlayerResourceData.skipAds <= 0)
            {
                ClaimFromAds();
            }
#endif
        }
        else
        {
            ClaimFromAds();
        }
    }

    private void OnClickClaimGold()
    {
        if (_isProcessing) return;
        ClaimFromGold();
    }

    /// <summary>
    /// Khi click vào claimAds sẽ cộng 1 heart vào data Accountplayer
    /// </summary>
    public void ClaimFromAds(bool autoClose = true)
    {
        if (_isProcessing) return;
        _isProcessing = true;

        if (claimAds != null) claimAds.interactable = false;
        if (claimGold != null) claimGold.interactable = false;

        var res = DataAccountPlayer.PlayerResourceData;
        if (res != null)
        {
            res.AddHeart(1);
            GameDebug.Log("[PopupGetMoreHeartController] Claimed 1 heart from Ads successfully");
        }

        UpdateUI();

        if (autoClose)
        {
            OnClosePopup();
        }
        else
        {
            _isProcessing = false;
            if (claimAds != null) claimAds.interactable = true;
            if (claimGold != null) claimGold.interactable = true;
        }
    }

    /// <summary>
    /// Khi click vào claimGold sẽ cộng valueHeart vào trong Accountplayer
    /// </summary>
    public bool ClaimFromGold(bool autoClose = true)
    {
        if (_isProcessing) return false;

        EnsureData();
        int price = _dataHeartController != null ? _dataHeartController.PriceHeart : 190;
        int value = _dataHeartController != null ? _dataHeartController.ValueHeart : 5;

        var res = DataAccountPlayer.PlayerResourceData;
        if (res == null) return false;

        if (price > 0 && res.gold < price)
        {
            if (!ToastManager.IsNull)
            {
                ToastManager.Instance.Show("Not enough gold!");
            }
            GameDebug.Log($"[PopupGetMoreHeartController] Not enough gold: needed {price}, current {res.gold}");
            return false;
        }

        _isProcessing = true;
        if (claimAds != null) claimAds.interactable = false;
        if (claimGold != null) claimGold.interactable = false;

        if (price > 0)
        {
            res.ChangeGoldValue(-price);
        }

        res.AddHeart(value);
        GameDebug.Log($"[PopupGetMoreHeartController] Bought {value} hearts with {price} gold successfully");

        UpdateUI();

        if (autoClose)
        {
            OnClosePopup();
        }
        else
        {
            _isProcessing = false;
            if (claimAds != null) claimAds.interactable = true;
            if (claimGold != null) claimGold.interactable = true;
        }

        return true;
    }
}
