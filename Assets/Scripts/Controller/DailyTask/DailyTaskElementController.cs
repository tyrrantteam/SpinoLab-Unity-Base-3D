using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DailyTaskElementController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI taskDescriptionTxt;
    [SerializeField] private Image taskProcessor;
    [SerializeField] private TextMeshProUGUI valueTaskTxt;
    [SerializeField] private ModuleRewardController rewardController;
    [SerializeField] private GameObject notiObject;
    [FormerlySerializedAs("claimObject")]
    [SerializeField] private GameObject claimedObject;
    [SerializeField] private Button claimBtn;

    private int currentValueTask;
    private ItemDailyTask _itemData;
    private Action _onClaimCompleted;

    private void Awake()
    {
        if (claimBtn == null)
            claimBtn = GetComponent<Button>();
        if (claimBtn == null)
            claimBtn = GetComponentInChildren<Button>();

        if (claimedObject == null)
        {
            var layer = transform.Find("LayerClaimEd");
            if (layer != null) claimedObject = layer.gameObject;
        }

        if (claimBtn != null)
        {
            claimBtn.onClick.RemoveAllListeners();
            claimBtn.onClick.AddListener(OnClickClaim);
        }
    }

    /// <summary>
    /// Load data qua hàm InitData
    /// </summary>
    public void InitData(ItemDailyTask item, Action onClaimCompleted = null)
    {
        _itemData = item;
        _onClaimCompleted = onClaimCompleted;

        if (item == null) return;

        // currentValueTask = data đc lưu của user dựa trên tiến độ task trong PlayerDailyTashkData
        currentValueTask = DataAccountPlayer.PlayerDailyTashkData.GetCurrentValue(item.typeTask);

        // taskDescriptionTxt = describeText truyền vào valueTask ví dụ : Play {0} games truyền vào value sẽ là Play 5 games
        if (taskDescriptionTxt != null)
        {
            if (!string.IsNullOrEmpty(item.describeText) && item.describeText.Contains("{0}"))
            {
                taskDescriptionTxt.text = string.Format(item.describeText, item.valueTask);
            }
            else
            {
                taskDescriptionTxt.text = $"{item.describeText} {item.valueTask}";
            }
        }

        // taskProcessor.fill = currentValueTask / valueTask
        float targetFill = item.valueTask > 0 ? Mathf.Clamp01((float)currentValueTask / item.valueTask) : 1f;
        if (taskProcessor != null)
        {
            taskProcessor.fillAmount = targetFill;
        }

        // valueTaskTxt = currentValueTask + "/" + valueTask
        if (valueTaskTxt != null)
        {
            valueTaskTxt.text = $"{currentValueTask}/{item.valueTask}";
        }

        // rewardController load reward từ data ItemDailyTask vào
        if (rewardController != null)
        {
            rewardController.gameObject.SetActive(true);
            if (!Enum.TryParse(item.typeResources, true, out TypeResources typeReward))
                typeReward = TypeResources.none;

            TypeRarity rarity = LoadResourceController.Instance != null
                ? LoadResourceController.Instance.loadRarityStaticByType(typeReward)
                : TypeRarity.normal;

            var data = new dataItem
            {
                typeReward = typeReward,
                typeRarity = rarity,
                value = item.value,
                level = 1
            };
            rewardController.InitData(data);
        }

        bool isClaimed = DataAccountPlayer.PlayerDailyTashkData.IsTaskClaimed(item.typeTask);
        bool isCompleted = currentValueTask >= item.valueTask;

        // claimedObject đc set active true khi đã claim xong task
        if (claimedObject != null)
        {
            claimedObject.SetActive(isClaimed);
        }

        // notiObject được set active true khi currentValueTask >= valueTask
        if (notiObject != null)
        {
            notiObject.SetActive(isCompleted && !isClaimed);
        }

        if (claimBtn != null)
        {
            claimBtn.interactable = !isClaimed && isCompleted;
        }
    }

    private void OnClickClaim()
    {
        if (_itemData == null) return;
        if (DataAccountPlayer.PlayerDailyTashkData.IsTaskClaimed(_itemData.typeTask)) return;
        if (currentValueTask < _itemData.valueTask) return;

        ClaimTask();
    }

    /// <summary>
    /// claimBtn bấm vào sẽ show màn PopupShowRewardSingleUnitController lên và cộng phần thưởng vào data
    /// </summary>
    public void ClaimTask()
    {
        if (_itemData == null) return;

        // 1. Lưu trạng thái đã claim
        DataAccountPlayer.PlayerDailyTashkData.SetTaskClaimed(_itemData.typeTask);

        // 2. Cộng phần thưởng vào data
        AddRewardToAccount(_itemData);

        // 3. Show PopupShowRewardSingleUnitController
        if (PopupManager.Instance != null)
        {
            var singleRewardPopup = PopupManager.Instance.ShowPopup<PopupShowRewardSingleUnitController>(popup =>
            {
                popup.ShowReward(_itemData.typeResources, _itemData.value, () =>
                {
                    _onClaimCompleted?.Invoke();
                });
            });

            if (singleRewardPopup == null)
            {
                singleRewardPopup = PopupManager.Instance.ModuleShowRewardSingleUnit();
                if (singleRewardPopup != null)
                {
                    singleRewardPopup.ShowReward(_itemData.typeResources, _itemData.value, () =>
                    {
                        _onClaimCompleted?.Invoke();
                    });
                }
            }
        }

        // Cập nhật lại UI sau khi claim
        InitData(_itemData, _onClaimCompleted);
        _onClaimCompleted?.Invoke();
    }

    public static void AddRewardToAccount(ItemDailyTask item)
    {
        if (item == null) return;
        RewardHelper.Grant(item.typeResources, item.value);
    }
}
