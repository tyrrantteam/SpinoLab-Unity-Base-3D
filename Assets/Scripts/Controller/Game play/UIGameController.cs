using Base.Core;
using JinGroup.UI.Common.Setting;
using LitMotion;
using LitMotion.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class UIGameController : SingletonMono<UIGameController>
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Button retryBtn;
    [SerializeField] private Button settingBtn;
    [SerializeField] private GameObject hardLevel;
    [SerializeField] private GameObject BottomBar;
    [SerializeField] private GameObject BannerAdsBar;

    public GameObject goldBar;
    [Title("Bottom bar slide")]
    [SerializeField] private float bottomBarSlideDuration = 0.4f;
    [SerializeField] private float bottomBarHideOffsetY = -180f;
    [SerializeField] private Ease bottomBarSlideEase = Ease.InOutCubic;

    [SerializeField] RectTransform _bottomBarRect;
    [SerializeField] Vector2 _bottomBarRestAnchoredPosition;
    private MotionHandle _bottomBarMotion;
    private const float BottomBarRestPositionEpsilon = 1f;

    protected override void Awake()
    {
        base.Awake();
        ListenerButton();
        _bottomBarRestAnchoredPosition = _bottomBarRect.anchoredPosition;
        var usingBannerAds = GameManager.Instance.DataAds.UsingBannerAds;
        BannerAdsBar.gameObject.SetActive(usingBannerAds);
    }

    [Button("TEST WARNING", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void TestWaringLevel()
    {
        SetWarningHardLevel();
    }

    [Button("TEST SlideUp", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void TestSlideUp()
    {
        BottomBarSlideUp();
    }

    [Button("TEST SlideDown", ButtonSizes.Large), GUIColor(0, 1, 0)]
    private void TestSlideDown()
    {
        BottomBarSlideDown();
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
        levelText.text = "LEVEL " + numberLevel.ToString();
        if(numberLevel == 5 || numberLevel == 6)
        {
            SetWarningHardLevel();
        }
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
