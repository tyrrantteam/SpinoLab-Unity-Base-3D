using Base.Core;
using JinGroup.UI.Common.Setting;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class UIGameController : SingletonMono<UIGameController>
{
    [SerializeField] private Text levelText;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private GameObject hardLevel;
    [SerializeField] private GameObject BottomBar;
    [SerializeField] private GameObject BannerAdsBar;

    [Title("Bottom bar slide")]
    [SerializeField] private float bottomBarSlideDuration = 0.4f;
    [SerializeField] private float bottomBarHideOffsetY = -180f;
    [SerializeField] private Ease bottomBarSlideEase = Ease.InOutCubic;

    private RectTransform _bottomBarRect;
    private Vector2 _bottomBarRestAnchoredPosition;
    private MotionHandle _bottomBarMotion;
    private const float BottomBarRestPositionEpsilon = 1f;

    protected override void Awake()
    {
        ListenerButton();
        _bottomBarRect = BottomBar.GetComponent<RectTransform>();
        _bottomBarRestAnchoredPosition = _bottomBarRect.anchoredPosition;
        var usingBannerAds = GameManager.Instance.DataAds.UsingBannerAds;
        BannerAdsBar.gameObject.SetActive(usingBannerAds);
    }

    [Button("TEST WARNING", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void TestWaringLevel()
    {
        SetWarningHardLevel();
    }

    protected virtual void ListenerButton()
    {
        retryBtn.onClick.AddListener(RetryGame);
        settingBtn.onClick.AddListener(OpenSetting);
    }

    public void OpenSetting()
    {
        PopupManager.Instance.ShowPopup<SettingController>();
    }

    public void RetryGame()
    {
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
    }

    public void SetLevelText(int numberLevel)
    {
        levelText.text = "LEVEL: " + numberLevel.ToString();
    }

    public void SetWarningHardLevel()
    {
        hardLevel.SetActive(true);
        SetWarningInGame();
    }

    public void SetWarningInGame()
    {
        this.PostEvent(EventID.ShowWarningHardLevel);
        HapticManager.Instance.PlayHapticWarning();
    }

    public void BottomBarSlideDown()
    {
        _bottomBarMotion.TryCancel();
        Vector2 from = _bottomBarRect.anchoredPosition;
        Vector2 to = _bottomBarRestAnchoredPosition + new Vector2(0f, bottomBarHideOffsetY);
        _bottomBarMotion = LMotion.Create(from, to, bottomBarSlideDuration)
            .WithEase(bottomBarSlideEase)
            .BindToAnchoredPosition(_bottomBarRect);
    }

    public void BottomBarSlideUp()
    {
        Vector2 pos = _bottomBarRect.anchoredPosition;
        float sqrDist = (pos - _bottomBarRestAnchoredPosition).sqrMagnitude;
        if (sqrDist <= BottomBarRestPositionEpsilon * BottomBarRestPositionEpsilon)
        {
            return;
        }

        _bottomBarMotion.TryCancel();
        Vector2 from = pos;
        Vector2 to = _bottomBarRestAnchoredPosition;
        _bottomBarMotion = LMotion.Create(from, to, bottomBarSlideDuration)
            .WithEase(bottomBarSlideEase)
            .BindToAnchoredPosition(_bottomBarRect);
    }
}
