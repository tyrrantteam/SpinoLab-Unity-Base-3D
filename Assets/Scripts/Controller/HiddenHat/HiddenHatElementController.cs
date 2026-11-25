using System;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using UnityEngine;
using UnityEngine.UI;

public class HiddenHatElementController : MonoBehaviour
{
    [SerializeField] private ModuleRewardController rewardController;
    [SerializeField] private Button claimRewardBtn;
    [SerializeField] private GameObject hatLayer;

    private ItemHiddenHat _itemData;
    private int _elementIndex = -1;
    private bool _isClaimed;
    private bool _isHatHidden;
    private Action<int> _onClaimStarted;
    private Action<int> _onClaimCompleted;
    private bool _isButtonBound;

    public ItemHiddenHat ItemData => _itemData;
    public bool IsClaimed => _isClaimed;
    public bool IsHatHidden => _isHatHidden;

    private void Awake()
    {
        BindButton();
    }

    private void BindButton()
    {
        if (_isButtonBound || claimRewardBtn == null)
            return;

        _isButtonBound = true;
        claimRewardBtn.onClick.RemoveListener(OnClickClaimReward);
        claimRewardBtn.onClick.AddListener(OnClickClaimReward);
    }

    public void InitData(
        ItemHiddenHat item,
        int elementIndex,
        bool isClaimed = false,
        bool isHatHidden = false,
        Action<int> onClaimStarted = null,
        Action<int> onClaimCompleted = null)
    {
        BindButton();

        _itemData = item;
        _elementIndex = elementIndex;
        _isClaimed = isClaimed;
        _isHatHidden = isHatHidden;
        _onClaimStarted = onClaimStarted;
        _onClaimCompleted = onClaimCompleted;

        InitRewardDisplay();

        if (_isClaimed || _isHatHidden)
        {
            HideHatLayerImmediate();
        }
        else
        {
            ShowHatLayerImmediate();
        }
    }

    public void SetClaimedState(bool claimed)
    {
        _isClaimed = claimed;
        if (claimed)
        {
            SetClaimInteractable(false);
        }
    }

    public void SetClaimInteractable(bool interactable)
    {
        if (claimRewardBtn != null)
        {
            claimRewardBtn.interactable = interactable;
        }
    }

    /// <summary>
    /// Hàm riêng ẩn hatLayer (có thể thêm logic animation ẩn hatLayer tại đây về sau).
    /// </summary>
    public void HideHatLayer(Action onComplete = null)
    {
        if (_isHatHidden)
        {
            SetClaimInteractable(false);
            onComplete?.Invoke();
            return;
        }

        _isHatHidden = true;
        SetClaimInteractable(false);

        if (hatLayer != null)
        {
            hatLayer.SetActive(false);
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// Ẩn hatLayer ngay lập tức không qua anim (dùng khi đồng bộ lại trạng thái đã lưu).
    /// </summary>
    public void HideHatLayerImmediate()
    {
        _isHatHidden = true;
        SetClaimInteractable(false);

        if (hatLayer != null)
        {
            hatLayer.SetActive(false);
        }
    }

    /// <summary>
    /// Hiện lại hatLayer ngay lập tức (dùng khi reset chu kỳ mới).
    /// </summary>
    public void ShowHatLayerImmediate()
    {
        _isHatHidden = false;
        SetClaimInteractable(!_isClaimed);

        if (hatLayer != null)
        {
            hatLayer.SetActive(true);
        }
    }

    private void InitRewardDisplay()
    {
        if (rewardController == null || _itemData == null)
            return;

        rewardController.gameObject.SetActive(true);

        if (!Enum.TryParse(_itemData.typeResources, true, out TypeResources typeReward))
            typeReward = TypeResources.none;

        TypeRarity rarity = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.loadRarityStaticByType(typeReward)
            : TypeRarity.normal;

        rewardController.InitData(new dataItem
        {
            typeReward = typeReward,
            typeRarity = rarity,
            value = _itemData.value,
            level = 1
        });
    }

    private void OnClickClaimReward()
    {
        if (_isClaimed || _isHatHidden || _itemData == null)
            return;

        _isClaimed = true;
        SetClaimInteractable(false);

        // Cộng reward vào database
        RewardHelper.Grant(_itemData.typeResources, _itemData.value);

        // Khóa tương tác các ô khác & lưu data trước khi chạy ẩn hatLayer
        _onClaimStarted?.Invoke(_elementIndex);

        // 1. Ẩn hatLayer của element đơn vừa claim trước
        // 2. Sau khi ẩn xong -> ẩn tất cả hatLayer của các item trên bàn và show PopupShowRewardSingleUnitController
        HideHatLayer(() =>
        {
            _onClaimCompleted?.Invoke(_elementIndex);

            if (PopupManager.Instance != null)
            {
                var singleRewardPopup = PopupManager.Instance.ModuleShowRewardSingleUnit();
                if (singleRewardPopup != null)
                {
                    singleRewardPopup.ShowReward(_itemData.typeResources, _itemData.value);
                }
            }
        });
    }
}
