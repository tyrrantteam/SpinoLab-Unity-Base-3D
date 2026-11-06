using Base.Core.Sound;
using JinGroup.Base.LoadData;
using JinGroup.Controller.Feature;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupShowReward : SingletonMono<PopupShowReward>
{
    [Title("Cài Đặt UI Popup")]
    [SerializeField] private Button closeBtn;
    [SerializeField] private Transform holderReward;
    [SerializeField] private ModuleRewardController ModuleRewardController;
    [SerializeField] private List<ModuleRewardController> listReward = new List<ModuleRewardController>();

    [Title("Hiệu Ứng Bắn Phần Thưởng")]
    [SerializeField] private RewardRayShootEffectUI shootEffect;

    private List<ItemData> _listitemData;
    private bool _isAnimationCompleted = false;
    private float _skipTimestamp = 0f;
    private Action _onClaimCallback;
    private bool _isHidingBoosters = false;

    public event Action OnPopupClosed;
    [SerializeField] private Text textclaim;

    protected override void Awake()
    {
        base.Awake();
        closeBtn.onClick.AddListener(OnClickScreen);
        InitData();
        if (textclaim != null)
        {
            textclaim.raycastTarget = false;
            textclaim.gameObject.SetActive(false);
        }
        if (shootEffect == null)
            shootEffect = GetComponent<RewardRayShootEffectUI>();
        Invoke("ActiveText", 0.5f);
    }

    private void InitData()
    {
        _listitemData = LoadResourceController.Instance.DataItemController().ContentContent.ListitemData;
    }
    private void ActiveText()
    {
        CancelInvoke(nameof(ActiveText));
        if (textclaim != null)
            textclaim.gameObject.SetActive(true);
    }
    public void OpenPopup(List<PopupReward> listBundlePackData, Action onClaimCallback = null)
    {
        // Reward trước chưa claim (mở chồng) — cộng luôn, không để gói đã mua bị mất.
        FlushPendingClaim();
        _onClaimCallback = onClaimCallback;
        _isHidingBoosters = false;
        gameObject.SetActive(true);
        HideAllRewards();

        _isAnimationCompleted = false;
        _skipTimestamp = 0f;

        if (listBundlePackData == null || listBundlePackData.Count == 0)
        {
            _isAnimationCompleted = true;
            ActiveText();
            return;
        }

        // Chuẩn bị danh sách phần thưởng theo data (ẩn ngay lập tức)
        List<ModuleRewardController> preparedRewards = new List<ModuleRewardController>();
        for (int i = 0; i < listBundlePackData.Count; i++)
        {
            var reward = GetOrCreateReward(i);
            reward.InitData(CreateDataItem(listBundlePackData[i]));
            reward.DisableLayer();

            // Luôn đặt đúng scale gốc từ Prefab (ví dụ: 1.3, 1.3, 1.3)
            if (ModuleRewardController != null)
            {
                reward.transform.localScale = ModuleRewardController.transform.localScale;
            }

            reward.gameObject.SetActive(false); // Đảm bảo ẩn hoàn toàn khi vừa mở popup
            preparedRewards.Add(reward);
        }

        // Ủy quyền hiệu ứng bắn tia cho RewardRayShootEffectUI
        if (shootEffect == null)
            shootEffect = GetComponentInChildren<RewardRayShootEffectUI>();

        if (shootEffect != null)
        {
            shootEffect.PlayShootRewards(preparedRewards, holderReward as RectTransform, () =>
            {
                _isAnimationCompleted = true;
                ActiveText();
            });
        }
        else
        {
            // Fallback nếu chưa gắn component hiệu ứng
            for (int i = 0; i < preparedRewards.Count; i++)
            {
                preparedRewards[i].gameObject.SetActive(true);
            }
            _isAnimationCompleted = true;
            ActiveText();
        }
    }

    private void OnClickScreen()
    {
        if (_isHidingBoosters)
            return;

        // Nếu anim chưa chạy xong (chưa hiện hết quà):
        if (!_isAnimationCompleted)
        {
            ActiveText();
            SkipAnimation();
            return;
        }

        // Chỉ khi nào hiện "tap to claim" thì mới cho tap nhận thưởng
        if (textclaim != null && !textclaim.gameObject.activeInHierarchy)
            return;

        // Chống double tap vô tình đóng ngay lập tức khi vừa nhấn skip
        if (Time.unscaledTime - _skipTimestamp < 0.15f)
            return;

        ClaimAndClose();
    }

    private void ClaimAndClose()
    {
        if (_isHidingBoosters)
            return;

        _isHidingBoosters = true;

        if (textclaim != null)
            textclaim.gameObject.SetActive(false);

        SoundManager.Instance.PlaySound(SoundType.button_click);

        var effect = _shootEffectOrSelf();
        if (effect != null)
        {
            effect.PlayHideRewards(FinishClaimAndClose);
        }
        else
        {
            FinishClaimAndClose();
        }
    }

    private void FinishClaimAndClose()
    {
        _isHidingBoosters = false;

        // Khi nào ẩn booster xong mới nhận phần thưởng
        FlushPendingClaim();

        Close();
    }

    private void FlushPendingClaim()
    {
        var cb = _onClaimCallback;
        _onClaimCallback = null;
        cb?.Invoke();
    }

    private void OnDisable()
    {
        // Bị tắt từ ngoài (đổi scene / popup khác) khi chưa tap claim — vẫn phải cộng thưởng.
        FlushPendingClaim();
        _isHidingBoosters = false;
    }

    public void SkipAnimation()
    {
        if (_isAnimationCompleted)
            return;

        _isAnimationCompleted = true;
        _skipTimestamp = Time.unscaledTime;

        ActiveText();

        if (_shootEffectOrSelf() != null)
        {
            _shootEffectOrSelf().SkipEffect();
        }
    }

    private ModuleRewardController GetOrCreateReward(int index)
    {
        if (index < listReward.Count && listReward[index] != null)
        {
            listReward[index].gameObject.SetActive(false);
            return listReward[index];
        }

        var reward = Instantiate(ModuleRewardController, holderReward);
        reward.gameObject.SetActive(false); 
        listReward.Add(reward);
        return reward;
    }

    private dataItem CreateDataItem(PopupReward popupReward)
    {
        var matchedItem = _listitemData != null
            ? _listitemData.Find(item => item.typeResources == popupReward.typeResources)
            : null;

        if (!Enum.TryParse(popupReward.typeResources, true, out TypeResources typeReward))
            typeReward = TypeResources.none;

        TypeRarity rarity = TypeRarity.normal;
        if (matchedItem != null && !string.IsNullOrEmpty(matchedItem.typeRarity))
            Enum.TryParse(matchedItem.typeRarity, true, out rarity);
        else
            rarity = LoadResourceController.Instance.loadRarityStaticByType(typeReward);

        return new dataItem
        {
            typeRarity = rarity,
            typeReward = typeReward,
            value = popupReward.value
        };
    }

    private void HideAllRewards()
    {
        _isHidingBoosters = false;

        if (_shootEffectOrSelf() != null)
            _shootEffectOrSelf().StopAllEffects();

        var ruongDo = GetComponentInChildren<RuongDo>();
        if (ruongDo != null)
            ruongDo.Stop();

        CancelInvoke(nameof(ActiveText));
        if (textclaim != null)
            textclaim.gameObject.SetActive(false);

        for (int i = 0; i < listReward.Count; i++)
        {
            if (listReward[i] != null)
                listReward[i].gameObject.SetActive(false);
        }

        // Ẩn toàn bộ child trong holderReward nếu có object mẫu từ prefab
        if (holderReward != null)
        {
            for (int i = 0; i < holderReward.childCount; i++)
            {
                var child = holderReward.GetChild(i);
                if (child != null)
                    child.gameObject.SetActive(false);
            }
        }
    }

    private void Close()
    {
        FlushPendingClaim();

        if (_shootEffectOrSelf() != null)
            _shootEffectOrSelf().StopAllEffects();

        HideAllRewards();
        gameObject.SetActive(false);
        if (PopupManager.Instance != null)
            PopupManager.Instance.CloseShowReward();
        OnPopupClosed?.Invoke();
    }

    private RewardRayShootEffectUI _shootEffectOrSelf()
    {
        if (shootEffect == null)
            shootEffect = GetComponentInChildren<RewardRayShootEffectUI>();
        return shootEffect;
    }
}
