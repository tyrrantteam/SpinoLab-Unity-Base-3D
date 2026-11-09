using System;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using UnityEngine;
using UnityEngine.UI;

public class PopupShowRewardSingleUnitController : PopupBaseController
{
    [SerializeField] private ModuleRewardController rewardClaim;
    [SerializeField] private Button claimBtn;

    private Action _onClaimCallback;

    public ModuleRewardController RewardClaim => rewardClaim;

    protected override void Awake()
    {
        // Yêu cầu 2: Cho phép show đè lên popup khác, khi đóng chỉ tắt chính nó
        keepOtherPopupsOnClose = true;
        base.Awake();

        if (rewardClaim == null)
            rewardClaim = GetComponentInChildren<ModuleRewardController>(true);
    }

    protected override void OnEnable()
    {
        // Yêu cầu 2: Đảm bảo popup luôn nằm ở đỉnh (last sibling) đè lên tất cả popup khác
        transform.SetAsLastSibling();
        base.OnEnable();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        if (claimBtn != null)
        {
            claimBtn.onClick.AddListener(OnClosePopup);
        }
    }

    /// <summary>
    /// Yêu cầu 1: Hàm truyền data reward vào và hiển thị rewardClaim lên
    /// </summary>
    public void ShowReward(dataItem data, Action onClaimCallback = null)
    {
        _onClaimCallback = onClaimCallback;

        if (rewardClaim != null)
        {
            rewardClaim.gameObject.SetActive(true);
            var texts = rewardClaim.GetComponentsInChildren<TMPro.TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null) texts[i].gameObject.SetActive(true);
            }
            rewardClaim.InitData(data);
        }

        transform.SetAsLastSibling();
        gameObject.SetActive(true);
    }

    public void ShowReward(PopupReward reward, Action onClaimCallback = null)
    {
        if (reward == null) return;
        if (!Enum.TryParse(reward.typeResources, true, out TypeResources type))
            type = TypeResources.none;

        TypeRarity rarity = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.loadRarityStaticByType(type)
            : TypeRarity.normal;

        ShowReward(new dataItem
        {
            typeReward = type,
            typeRarity = rarity,
            value = reward.value,
            level = 1
        }, onClaimCallback);
    }

    public void ShowReward(ItemLuckySpin item, Action onClaimCallback = null)
    {
        if (item == null) return;
        if (!Enum.TryParse(item.typeResources, true, out TypeResources type))
            type = TypeResources.none;

        TypeRarity rarity = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.loadRarityStaticByType(type)
            : TypeRarity.normal;

        ShowReward(new dataItem
        {
            typeReward = type,
            typeRarity = rarity,
            value = item.value,
            level = 1
        }, onClaimCallback);
    }

    public void ShowReward(TypeResources type, int value, Action onClaimCallback = null)
    {
        TypeRarity rarity = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.loadRarityStaticByType(type)
            : TypeRarity.normal;

        ShowReward(new dataItem
        {
            typeReward = type,
            typeRarity = rarity,
            value = value,
            level = 1
        }, onClaimCallback);
    }

    public void ShowReward(string typeResources, int value, Action onClaimCallback = null)
    {
        if (!Enum.TryParse(typeResources, true, out TypeResources type))
            type = TypeResources.none;

        TypeRarity rarity = LoadResourceController.Instance != null
            ? LoadResourceController.Instance.loadRarityStaticByType(type)
            : TypeRarity.normal;

        ShowReward(new dataItem
        {
            typeReward = type,
            typeRarity = rarity,
            value = value,
            level = 1
        }, onClaimCallback);
    }

    // Các alias OpenPopup tương thích
    public void OpenPopup(dataItem data, Action onClaimCallback = null) => ShowReward(data, onClaimCallback);
    public void OpenPopup(PopupReward reward, Action onClaimCallback = null) => ShowReward(reward, onClaimCallback);
    public void OpenPopup(ItemLuckySpin item, Action onClaimCallback = null) => ShowReward(item, onClaimCallback);
    public void OpenPopup(TypeResources type, int value, Action onClaimCallback = null) => ShowReward(type, value, onClaimCallback);

    protected override void OnClosed()
    {
        base.OnClosed();
        var callback = _onClaimCallback;
        _onClaimCallback = null;
        callback?.Invoke();
    }
}
