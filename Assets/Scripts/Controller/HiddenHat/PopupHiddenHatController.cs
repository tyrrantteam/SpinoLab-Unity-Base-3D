using System;
using System.Collections;
using System.Collections.Generic;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.UI.SwipeHub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupHiddenHatController : PopupBaseController
{
    [SerializeField] private Button claimAllBtn;
    [SerializeField] private TextMeshProUGUI priceTxt;
    [SerializeField] private TextMeshProUGUI timeCounting;
    [SerializeField] private List<HiddenHatElementController> hiddenHatElements;

    private DataHiddenHatController _dataHiddenHat;
    private Coroutine _countdownRoutine;
    private int _currentPrice;

    protected override void OnEnable()
    {
        base.OnEnable();
        InitData();

        if (_countdownRoutine != null)
            StopCoroutine(_countdownRoutine);
        _countdownRoutine = StartCoroutine(CountdownRoutine());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_countdownRoutine != null)
        {
            StopCoroutine(_countdownRoutine);
            _countdownRoutine = null;
        }
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        if (claimAllBtn != null)
        {
            claimAllBtn.onClick.RemoveListener(OnClickClaimAll);
            claimAllBtn.onClick.AddListener(OnClickClaimAll);
        }
    }

    protected override void InitData()
    {
        base.InitData();
        EnsureDataLoaded();

        if (_dataHiddenHat == null)
        {
            Debug.LogWarning("[PopupHiddenHatController] Không tìm thấy DataHiddenHatController.");
            return;
        }

        var playerData = DataAccountPlayer.PlayerHiddenHatData;
        playerData.CheckAndResetIfExpired();

        if (!TryGetOrRandomGroupId(playerData, out int groupId))
        {
            Debug.LogWarning("[PopupHiddenHatController] Không có dữ liệu group trong DataHiddenHatController.");
            return;
        }

        List<ItemHiddenHat> groupRewards = _dataHiddenHat.GetRewardsByGroupId(groupId);
        if (groupRewards.Count == 0)
        {
            Debug.LogWarning($"[PopupHiddenHatController] Group ID {groupId} không có item reward nào.");
            return;
        }

        _currentPrice = Mathf.Max(0, _dataHiddenHat.GetPriceClaimAll(groupId));

        BindRewardsToElements(playerData, groupRewards);
        UpdatePriceUI();
        UpdateClaimAllButtonState();
        UpdateCountdownText();
    }

    private void EnsureDataLoaded()
    {
        if (_dataHiddenHat != null)
            return;

        if (LoadResourceController.Instance != null)
            _dataHiddenHat = LoadResourceController.Instance.DataHiddenHatController();

        if (_dataHiddenHat == null)
            _dataHiddenHat = Resources.Load<DataHiddenHatController>("Data/DataHiddenHat");
    }

    /// <summary>
    /// Lấy group ID hiện tại nếu đang trong chu kỳ 24h, hoặc random group ID mới không trùng với các lần trước.
    /// </summary>
    private bool TryGetOrRandomGroupId(PlayerHiddenHatData playerData, out int groupId)
    {
        List<int> allGroupIds = _dataHiddenHat.GetAllGroupIds();
        if (allGroupIds.Count == 0)
        {
            groupId = -1;
            return false;
        }

        // Nếu vẫn còn trong 24h và group hiện tại hợp lệ thì dùng lại
        if (playerData.hasActiveGroup && allGroupIds.Contains(playerData.currentGroupId))
        {
            groupId = playerData.currentGroupId;
            return true;
        }

        // Lọc các group ID chưa từng random
        List<int> availableGroupIds = new List<int>();
        for (int i = 0; i < allGroupIds.Count; i++)
        {
            int id = allGroupIds[i];
            if (playerData.usedGroupIds == null || !playerData.usedGroupIds.Contains(id))
            {
                availableGroupIds.Add(id);
            }
        }

        // Nếu đã random qua hết tất cả group -> reset lịch sử để xoay vòng lại (tránh trùng ngay group vừa trước đó)
        if (availableGroupIds.Count == 0)
        {
            int previousGroupId = playerData.currentGroupId;
            playerData.ResetUsedGroupHistory();

            for (int i = 0; i < allGroupIds.Count; i++)
            {
                if (allGroupIds.Count > 1 && allGroupIds[i] == previousGroupId)
                    continue;

                availableGroupIds.Add(allGroupIds[i]);
            }
        }

        groupId = availableGroupIds[UnityEngine.Random.Range(0, availableGroupIds.Count)];
        float durationHours = _dataHiddenHat.GetTimeHours(groupId, 24f);
        playerData.SetCurrentGroup(groupId, durationHours);
        return true;
    }

    /// <summary>
    /// Import data từ DataHiddenHatController và init vào hiddenHatElements theo thứ tự ngẫu nhiên nhưng đủ list.
    /// </summary>
    private void BindRewardsToElements(PlayerHiddenHatData playerData, List<ItemHiddenHat> groupRewards)
    {
        if (hiddenHatElements == null || hiddenHatElements.Count == 0)
            return;

        List<int> order = playerData.shuffledRewardIndices;
        if (!IsValidPermutation(order, groupRewards.Count))
        {
            order = CreateShuffledIndices(groupRewards.Count);
            playerData.SetShuffledIndices(order);
        }

        int slotCount = hiddenHatElements.Count;
        int activeCount = Mathf.Min(slotCount, order.Count);

        for (int i = 0; i < slotCount; i++)
        {
            var element = hiddenHatElements[i];
            if (element == null) continue;

            if (i < activeCount)
            {
                int rewardIndex = order[i];
                ItemHiddenHat rewardItem = groupRewards[rewardIndex];
                bool isClaimed = playerData.IsElementClaimed(i);
                bool isHatHidden = isClaimed || playerData.isBoardRevealed;

                element.gameObject.SetActive(true);
                element.InitData(
                    rewardItem,
                    i,
                    isClaimed,
                    isHatHidden,
                    OnElementClaimStarted,
                    OnElementClaimCompleted);
            }
            else
            {
                element.gameObject.SetActive(false);
            }
        }
    }

    private bool IsValidPermutation(List<int> indices, int rewardCount)
    {
        if (indices == null || indices.Count != rewardCount || rewardCount <= 0)
            return false;

        bool[] seen = new bool[rewardCount];
        for (int i = 0; i < rewardCount; i++)
        {
            int val = indices[i];
            if (val < 0 || val >= rewardCount || seen[val])
                return false;
            seen[val] = true;
        }

        return true;
    }

    private List<int> CreateShuffledIndices(int count)
    {
        List<int> indices = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            indices.Add(i);
        }

        // Fisher-Yates shuffle: đảm bảo random không theo thứ tự gốc và đủ toàn bộ phần tử
        for (int i = count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        return indices;
    }

    private void OnElementClaimStarted(int elementIndex)
    {
        var playerData = DataAccountPlayer.PlayerHiddenHatData;
        playerData.MarkElementClaimed(elementIndex);
        playerData.SetBoardRevealed(true);

        // Khóa nút chọn ở các element khác trong khi ô đang claim chạy ẩn hatLayer
        if (hiddenHatElements == null)
            return;

        for (int i = 0; i < hiddenHatElements.Count; i++)
        {
            if (hiddenHatElements[i] != null)
            {
                hiddenHatElements[i].SetClaimInteractable(false);
            }
        }
    }

    private void OnElementClaimCompleted(int elementIndex)
    {
        // Sau khi ẩn hatLayer đơn xong -> ẩn tất cả hatLayer của các item trên bàn
        HideAllHatLayers(skipIndex: elementIndex);
        UpdateClaimAllButtonState();
    }

    /// <summary>
    /// Ẩn toàn bộ hatLayer của các item đang có trong bàn.
    /// </summary>
    public void HideAllHatLayers(int skipIndex = -1)
    {
        if (hiddenHatElements == null)
            return;

        for (int i = 0; i < hiddenHatElements.Count; i++)
        {
            if (i == skipIndex)
                continue;

            var element = hiddenHatElements[i];
            if (element == null || !element.gameObject.activeSelf)
                continue;

            element.HideHatLayer();
        }
    }

    private void UpdatePriceUI()
    {
        if (priceTxt == null)
            return;

        priceTxt.text = _currentPrice > 0 ? _currentPrice.ToString() : "Claim All";
    }

    private bool HasUnclaimedElements()
    {
        if (hiddenHatElements == null)
            return false;

        for (int i = 0; i < hiddenHatElements.Count; i++)
        {
            var element = hiddenHatElements[i];
            if (element != null && element.gameObject.activeSelf && !element.IsClaimed)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateClaimAllButtonState()
    {
        if (claimAllBtn == null)
            return;

        claimAllBtn.interactable = HasUnclaimedElements();
    }

    /// <summary>
    /// Khi bấm vào claimAllBtn:
    /// - Nếu gold &lt; price: tắt popup và chuyển sang màn hình Shop ở Home qua tab button (SwipePageHub).
    /// - Nếu gold &gt;= price: trừ price khỏi gold trong PlayerResourceData, nhận hết các reward chưa claim và show PopupShowReward.
    /// </summary>
    private void OnClickClaimAll()
    {
        if (_isClosing || !HasUnclaimedElements())
            return;

        var resourceData = DataAccountPlayer.PlayerResourceData;
        if (resourceData.gold < _currentPrice)
        {
            _canClose = true;
            OnClosePopup();

            if (SwipePageHub.Instance != null)
            {
                SwipePageHub.Instance.SelectPage(SwipePageKind.Shop);
            }
            return;
        }

        if (_currentPrice > 0)
        {
            resourceData.ChangeGoldValue(-_currentPrice);
        }

        var playerData = DataAccountPlayer.PlayerHiddenHatData;
        List<PopupReward> rewardsToShow = new List<PopupReward>();

        for (int i = 0; i < hiddenHatElements.Count; i++)
        {
            var element = hiddenHatElements[i];
            if (element == null || !element.gameObject.activeSelf)
                continue;

            if (element.IsClaimed || element.ItemData == null)
                continue;

            ItemHiddenHat item = element.ItemData;

            element.SetClaimedState(true);
            element.HideHatLayer();
            playerData.MarkElementClaimed(i);

            // Cộng phần thưởng vào database
            RewardHelper.Grant(item.typeResources, item.value);

            rewardsToShow.Add(new PopupReward
            {
                id = item.id,
                typeResources = item.typeResources,
                value = item.value
            });
        }

        playerData.SetBoardRevealed(true);
        UpdateClaimAllButtonState();

        if (rewardsToShow.Count > 0 && PopupManager.Instance != null)
        {
            var showRewardPopup = PopupManager.Instance.ModuleShowReward();
            if (showRewardPopup != null)
            {
                showRewardPopup.OpenPopup(rewardsToShow);
            }
        }
    }

    private IEnumerator CountdownRoutine()
    {
        var wait = new WaitForSecondsRealtime(1f);
        while (true)
        {
            var playerData = DataAccountPlayer.PlayerHiddenHatData;
            if (playerData.CheckAndResetIfExpired())
            {
                InitData();
            }
            else
            {
                UpdateCountdownText();
            }

            yield return wait;
        }
    }

    private void UpdateCountdownText()
    {
        if (timeCounting == null)
            return;

        TimeSpan remaining = DataAccountPlayer.PlayerHiddenHatData.GetRemainingTime();
        int totalHours = Mathf.Max(0, (int)remaining.TotalHours);
        timeCounting.text = $"reset after : {totalHours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }
}
