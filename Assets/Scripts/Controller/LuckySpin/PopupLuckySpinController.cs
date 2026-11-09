using Base.Core.Debug;
using Base.Core.Sound;
using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using LitMotion;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupLuckySpinController : PopupBaseController
{
    [Title("Buttons")]
    [SerializeField] private Button spinBtn;
    [SerializeField] private Button spinAdsBtn;

    [Title("Progress Views")]
    [SerializeField] private Image loadingProcessor;
    [SerializeField] private TextMeshProUGUI valueProcessorTxt;
    [SerializeField] private TextMeshProUGUI textDescriptionTxt;

    [Title("Spin Visual Elements")]
    [SerializeField] private GameObject coreSpin;
    [SerializeField] private GameObject decor;
    [SerializeField] private GameObject lightDecor1;
    [SerializeField] private GameObject lightDecor2;

    [Title("Reward Slots & Formation")]
    [SerializeField] private List<ModuleRewardController> rewardControllers = new List<ModuleRewardController>();
    [SerializeField] private GameObject[] circlingFormation;
    [SerializeField] private float distanceFormation = 250f;

    [Title("Spin Settings")]
    [SerializeField] private int spinRounds = 5;
    [SerializeField] private float spinDuration = 4.5f;
    [SerializeField] private float overshootAngle = 8f;
    [SerializeField] private float decorKickAngle = 18f;
    [SerializeField] private float decorKickDuration = 0.1f;

    private DataLuckySpinController _dataLuckySpinController;
    private int _mileStoneSpin;
    private bool _isSpinning;
    private MotionHandle _spinMotion;
    private MotionHandle _decorMotion;

    protected override void InitData()
    {
        base.InitData();

        if (_dataLuckySpinController == null)
        {
            if (!LoadResourceController.IsNull)
                _dataLuckySpinController = LoadResourceController.Instance.DataLuckySpinController();
            if (_dataLuckySpinController == null)
                _dataLuckySpinController = Resources.Load<DataLuckySpinController>("Data/DataLuckySpin");
        }

        _mileStoneSpin = _dataLuckySpinController != null ? _dataLuckySpinController.MileStoneSpin : 5;

        InitRewardControllers();
        UpdateUI();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _isSpinning = false;
        InitData();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _spinMotion.TryCancel();
        _decorMotion.TryCancel();
        _isSpinning = false;
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();

        if (spinBtn != null)
            spinBtn.onClick.AddListener(OnClickSpinBtn);

        if (spinAdsBtn != null)
            spinAdsBtn.onClick.AddListener(OnClickSpinAdsBtn);
    }

    [Button("Arrange Formation", ButtonSizes.Medium), GUIColor(0.2f, 0.8f, 0.2f)]
    public void ArrangeFormation()
    {
        if (circlingFormation == null || circlingFormation.Length == 0) return;

        int count = circlingFormation.Length;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            var obj = circlingFormation[i];
            if (obj == null) continue;

            // Bắt đầu từ 12h (90 độ) và chia đều theo chiều kim đồng hồ (- i * angleStep)
            float angleDeg = 90f - i * angleStep;
            float rad = angleDeg * Mathf.Deg2Rad;
            float x = distanceFormation * Mathf.Cos(rad);
            float y = distanceFormation * Mathf.Sin(rad);

            var rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(x, y);
                rt.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            }
            else
            {
                obj.transform.localPosition = new Vector3(x, y, obj.transform.localPosition.z);
                obj.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg - 90f);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }
    }

    private void InitRewardControllers()
    {
        if (_dataLuckySpinController == null || _dataLuckySpinController.ContentContent == null) return;
        var listData = _dataLuckySpinController.ContentContent.ListitemData;
        if (listData == null || listData.Count == 0) return;

        for (int i = 0; i < rewardControllers.Count; i++)
        {
            if (rewardControllers[i] == null) continue;

            if (i < listData.Count)
            {
                var item = listData[i];
                if (!Enum.TryParse(item.typeResources, true, out TypeResources typeReward))
                    typeReward = TypeResources.none;

                var rarity = LoadResourceController.Instance != null
                    ? LoadResourceController.Instance.loadRarityStaticByType(typeReward)
                    : TypeRarity.normal;

                var dataItem = new dataItem
                {
                    typeReward = typeReward,
                    typeRarity = rarity,
                    value = item.value,
                    level = item.id
                };

                rewardControllers[i].InitData(dataItem);
                rewardControllers[i].gameObject.SetActive(true);
            }
            else
            {
                rewardControllers[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateUI()
    {
        int currentLevelProcess = DataAccountPlayer.PlayerLuckySpinData.currentLevelProcess;

        if (_mileStoneSpin <= 0) _mileStoneSpin = 5;

        // 7: loadingProcessor.fill = currentLevelProcess / _mileStoneSpin
        if (loadingProcessor != null)
        {
            loadingProcessor.fillAmount = Mathf.Clamp01((float)currentLevelProcess / _mileStoneSpin);
        }

        // 8: valueProcessorTxt = currentLevelProcess + / + _mileStoneSpin
        if (valueProcessorTxt != null)
        {
            valueProcessorTxt.text = $"{currentLevelProcess}/{_mileStoneSpin}";
        }

        // 9: textDescriptionTxt: Win (mileStone - current) to Spin nếu mileStone > current, ngược lại Feeling Lucky ?
        if (textDescriptionTxt != null)
        {
            if (_mileStoneSpin > currentLevelProcess)
            {
                int diff = _mileStoneSpin - currentLevelProcess;
                textDescriptionTxt.text = $"Win {diff} to Spin";
            }
            else
            {
                textDescriptionTxt.text = "Feeling Lucky ?";
            }
        }

        UpdateButtonState(currentLevelProcess);
    }

    private void UpdateButtonState(int currentLevelProcess)
    {
        bool canSpinNormal = currentLevelProcess >= _mileStoneSpin && !_isSpinning;
        if (spinBtn != null)
        {
            spinBtn.interactable = canSpinNormal;
        }

        if (spinAdsBtn != null)
        {
            spinAdsBtn.interactable = !_isSpinning;
        }
    }

    private void OnClickSpinBtn()
    {
        if (_isSpinning) return;

        // 12: khi bấm spinBtn sẽ check nếu _mileStoneSpin <= currentLevelProcess thì ms call hàm spin
        int currentProcess = DataAccountPlayer.PlayerLuckySpinData.currentLevelProcess;
        if (_mileStoneSpin <= currentProcess)
        {
            Spin();
        }
    }

    private void OnClickSpinAdsBtn()
    {
        if (_isSpinning) return;

        // 13: khi bấm spinAdsBtn sẽ call show adsreward rồi ms call hàm spin
        if (!AdsManager.IsNull)
        {
            AdsManager.Instance.ShowRewarded(() =>
            {
                Spin();
            });
#if UNITY_EDITOR
            // Fallback khi test trên Unity Editor nếu Ads SDK chưa uncomment callback
            if (DataAccountPlayer.PlayerResourceData.skipAds <= 0)
            {
                Spin();
            }
#endif
        }
        else
        {
            Spin();
        }
    }

    public void Spin()
    {
        if (_isSpinning) return;
        DataAccountPlayer.PlayerDailyTashkData.UpdateTask(TypeTaskDaily.PlayLuckySpin);
        var listData = _dataLuckySpinController?.ContentContent?.ListitemData;
        if (listData == null || listData.Count == 0)
        {
            GameDebug.LogError("[LuckySpin] Dữ liệu LuckySpin rỗng, không thể quay!");
            return;
        }

        int slotCount = circlingFormation != null && circlingFormation.Length > 0
            ? circlingFormation.Length
            : (rewardControllers.Count > 0 ? rewardControllers.Count : listData.Count);

        if (slotCount <= 0) slotCount = 8;

        // 10.3: Trước khi spin đã phải random ra kết quả ô reward dựa trên rate theo trọng số
        float totalRate = 0f;
        for (int i = 0; i < listData.Count; i++)
        {
            totalRate += listData[i].rate > 0 ? listData[i].rate : 1f;
        }

        float randomPoint = UnityEngine.Random.Range(0f, totalRate);
        int targetIndex = 0;
        float currentSum = 0f;
        for (int i = 0; i < listData.Count; i++)
        {
            currentSum += listData[i].rate > 0 ? listData[i].rate : 1f;
            if (randomPoint <= currentSum)
            {
                targetIndex = i;
                break;
            }
        }

        ItemLuckySpin chosenItem = listData[targetIndex];

        // Khóa tương tác
        _isSpinning = true;
        if (spinBtn != null) spinBtn.interactable = false;
        if (spinAdsBtn != null) spinAdsBtn.interactable = false;

        float angleStep = 360f / slotCount;
        float startAngle = coreSpin != null ? coreSpin.transform.localEulerAngles.z : 0f;

        // Góc mục tiêu: quay theo chiều kim đồng hồ (Z âm), ô targetIndex về đỉnh (12h)
        float currentAngleNorm = (startAngle % 360f + 360f) % 360f;
        float targetAngleNorm = ((targetIndex * angleStep) % 360f + 360f) % 360f;
        float clockwiseDiff = (currentAngleNorm - targetAngleNorm + 360f) % 360f;
        float totalClockwiseAngle = (spinRounds * 360f) + clockwiseDiff;
        float finalTargetAngle = startAngle - totalClockwiseAngle;

        // 10.2: Xoay dướn lên trên 1 chút (-overshootAngle) rồi tụt lại về đúng ô
        float overshootTargetAngle = finalTargetAngle - overshootAngle;

        float mainDuration = Mathf.Max(0.5f, spinDuration * 0.9f);
        float bounceDuration = Mathf.Max(0.2f, spinDuration * 0.1f);

        int lastPassIndex = 0;

        _spinMotion.TryCancel();

        // Pha 1: Quay chậm dần và dướn lố qua đích một chút
        _spinMotion = LMotion.Create(startAngle, overshootTargetAngle, mainDuration)
            .WithEase(Ease.OutQuad)
            .WithOnComplete(() =>
            {
                // Pha 2: Tụt lùi đàn hồi về đúng ô
                _spinMotion = LMotion.Create(overshootTargetAngle, finalTargetAngle, bounceDuration)
                    .WithEase(Ease.InOutQuad)
                    .WithOnComplete(() =>
                    {
                        OnSpinCompleted(chosenItem);
                    })
                    .Bind(currentZ =>
                    {
                        if (coreSpin != null)
                            coreSpin.transform.localRotation = Quaternion.Euler(0f, 0f, currentZ);
                    });
            })
            .Bind(currentZ =>
            {
                if (coreSpin != null)
                    coreSpin.transform.localRotation = Quaternion.Euler(0f, 0f, currentZ);

                // 10.4: Khi đang spin mỗi lần 1 ô reward quay qua đỉnh decor sẽ lắc 1 cái theo chiều vòng quay
                float totalRotated = Mathf.Abs(currentZ - startAngle);
                int currentPassIndex = Mathf.FloorToInt(totalRotated / angleStep);
                if (currentPassIndex > lastPassIndex)
                {
                    lastPassIndex = currentPassIndex;
                    PlayDecorKick();
                    ToggleLights(currentPassIndex);
                }
            });
    }

    private void PlayDecorKick()
    {
        if (decor == null) return;

        _decorMotion.TryCancel();
        // Lắc theo chiều quay (âm) rồi bật lại 0
        _decorMotion = LMotion.Create(-decorKickAngle, 0f, decorKickDuration)
            .WithEase(Ease.OutQuad)
            .Bind(z =>
            {
                if (decor != null)
                    decor.transform.localRotation = Quaternion.Euler(0f, 0f, z);
            });

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(SoundType.ClickButton);
        }
    }

    private void ToggleLights(int step)
    {
        if (lightDecor1 != null) lightDecor1.SetActive(step % 2 == 0);
        if (lightDecor2 != null) lightDecor2.SetActive(step % 2 != 0);
    }

    private void OnSpinCompleted(ItemLuckySpin chosenItem)
    {
        _isSpinning = false;

        // 10.5: Trả phần thưởng nhận được và cộng tài nguyên vào dataAccountPlayer
        AddRewardToAccount(chosenItem);

        // Yêu cầu 3: Thay thế logic trả thưởng qua popup cũ thành popup PopupShowRewardSingleUnitController
        if (PopupManager.Instance != null)
        {
            var singleRewardPopup = PopupManager.Instance.ShowPopup<PopupShowRewardSingleUnitController>(popup =>
            {
                popup.ShowReward(chosenItem);
            });

            if (singleRewardPopup == null)
            {
                singleRewardPopup = PopupManager.Instance.ModuleShowRewardSingleUnit();
                if (singleRewardPopup != null)
                {
                    singleRewardPopup.ShowReward(chosenItem);
                }
            }
        }

        // 11: sau khi spin xong sẽ reset lại currentLevelProcess = 0 và valueProcessorTxt, textDescriptionTxt
        DataAccountPlayer.PlayerLuckySpinData.ResetProcess();
        UpdateUI();
    }

    private void AddRewardToAccount(ItemLuckySpin item)
    {
        if (item == null) return;
        if (!Enum.TryParse(item.typeResources, true, out TypeResources type))
            return;

        var res = DataAccountPlayer.PlayerResourceData;
        if (res == null) return;

        switch (type)
        {
            case TypeResources.gold:
                res.ChangeGoldValue(item.value);
                break;
            case TypeResources.diamond:
                res.ChangeDiamondValue(item.value);
                break;
            case TypeResources.skipAds:
                res.ChangeSkipAdsValue(item.value);
                break;
            case TypeResources.heart:
                res.AddHeart(item.value);
                break;
            case TypeResources.heartHour:
                res.AddInfiniteHeartHours(item.value);
                break;
            case TypeResources.heartDay:
                res.AddInfiniteHeartDays(item.value);
                break;
            case TypeResources.booster1:
                res.ChangeBoosterCount(BoosterType.booster1, item.value);
                break;
            case TypeResources.booster2:
                res.ChangeBoosterCount(BoosterType.booster2, item.value);
                break;
            case TypeResources.booster3:
                res.ChangeBoosterCount(BoosterType.booster3, item.value);
                break;
            case TypeResources.noads:
                res.ChangeNoAdsStatus(true);
                break;
            case TypeResources.noads24h:
                res.ChangeNoAds24hStatus(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                break;
        }

        DataAccountPlayer.SavePlayerResourceData();
        GameDebug.Log($"[LuckySpin] Đã nhận thưởng: {type} x{item.value}");
    }
}
