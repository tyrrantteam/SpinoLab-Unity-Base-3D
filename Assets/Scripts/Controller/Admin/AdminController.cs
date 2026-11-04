using Base.Core;
using Base.Core.Debug;
using DataAccount;
using System.Collections;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AdminController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button panelAdmin;
    [SerializeField] private Button openAdmin;
    [Header("Level")]
    [SerializeField] private Button btnWin;
    [SerializeField] private Button btnLose;
    [SerializeField] private Button btnCheat;
    [SerializeField] private InputField inputLevel;
    [Header("Booster")]
    [SerializeField] private Button UnlockAllBooster;
    [SerializeField] private Button Add10BoosterEachType;
    [Header("Resource")]
    [SerializeField] private Button add1000Gold;
    [SerializeField] private Button add1000Heart;
    [SerializeField] private Button add10SkipAds;
    [SerializeField] private Button activeInfiniteHeart1h;
    [Header("StreakUI")]
    [SerializeField] private Button btnFinishStreak;
    [SerializeField] private InputField inputDayStreak;
    [SerializeField] private Button btnMoveToDayStreak;
    [Header("Marketing Video")]
    [SerializeField] private Button DisableUI;
    [SerializeField] private Button ActiveUI;
    [SerializeField] private Button activeHand;
    [SerializeField] private Button deActiveHand;
    [SerializeField] private GameObject hand;
    [SerializeField] private GameObject UIGameHeader;
    [SerializeField] private GameObject UIGameBottomBar;
    [Header("ADS in Game")]
    [SerializeField] private Button btnShowInter;
    [SerializeField] private Button btnShowBanner;
    [SerializeField] private Text timeToNextInter;
    public int defaultValue = 1;

    private Coroutine _updateTimeCoroutine;

    private void Awake()
    {
        openAdmin.onClick.AddListener(OpenAdmin);
        ListenerBtn();
    }

    private void ListenerBtn()
    {
        btnWin.onClick.AddListener(OnClickWin);
        btnLose.onClick.AddListener(OnClickLose);
        btnCheat.onClick.AddListener(OnClickCheat);
        panelAdmin.onClick.AddListener(CloseAdmin);
        inputLevel.onEndEdit.AddListener(OnEndEdit);
        UnlockAllBooster.onClick.AddListener(OnUnlockAllBooster);
        Add10BoosterEachType.onClick.AddListener(OnAdd10BoosterEachType);

        DisableUI.onClick.AddListener(OnDisableGameUiForMarketing);
        ActiveUI.onClick.AddListener(OnActiveGameUiForMarketing);
        activeHand.onClick.AddListener(OnActiveHand);
        deActiveHand.onClick.AddListener(OnDeActiveHand);

        btnMoveToDayStreak.onClick.AddListener(OnMoveToDayStreak);
        btnFinishStreak.onClick.AddListener(OnFinishStreak);

        add1000Gold.onClick.AddListener(OnAdd1000Gold);
        add1000Heart.onClick.AddListener(OnAdd1000Heart);
        add10SkipAds.onClick.AddListener(OnAdd10SkipAds);
        activeInfiniteHeart1h.onClick.AddListener(OnActiveInfiniteHeart1h);

        if (btnShowInter != null) btnShowInter.onClick.AddListener(OnShowInter);
        if (btnShowBanner != null) btnShowBanner.onClick.AddListener(OnShowBanner);
    }

    private void Start()
    {
        panelAdmin.gameObject.SetActive(false);
        openAdmin.interactable = !GameDebug.IsProduction;
    }

    private void OnDisable()
    {
        StopUpdateTimeCoroutine();
    }

    #region GameServices
    public void OpenAdmin()
    {
        if (GameDebug.IsProduction)
        {
            GameDebug.LogWarning("Admin panel is disabled in production mode.");
            return;
        }
        panelAdmin.gameObject.SetActive(true);
        StartUpdateTimeCoroutine();
    }

    public void CloseAdmin()
    {
        StopUpdateTimeCoroutine();
        panelAdmin.gameObject.SetActive(false);
    }

    private void OnClickWin()
    {
        GameController.Instance.SetWinGame();
    }

    private void OnClickLose()
    {
        GameController.Instance.SetLoseGame();
    }

    void OnEndEdit(string text)
    {
        int value;
        if (int.TryParse(text, out value))
        {
            Debug.Log("Giá trị int: " + value);
            DataAccountPlayer.PlayerPointProcessData.SetCurrentLevel(value);
            DataAccountPlayer.PlayerPointProcessData.SetCurrrentDataShow(value);
        }
        else
        {
            GameDebug.LogWarning("Giá trị không hợp lệ! Reset về mặc định.");
            inputLevel.text = defaultValue.ToString();
            DataAccountPlayer.PlayerPointProcessData.SetCurrentLevel(defaultValue);
            DataAccountPlayer.PlayerPointProcessData.SetCurrrentDataShow(defaultValue);
        }
    }

    private void OnClickCheat()
    {
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
    }

    private void OnUnlockAllBooster()
    {
        DataAccountPlayer.PlayerPointProcessData.SetCheatUnlockAllBoosters(true);
        RefreshBoosterUi();
    }

    private void OnAdd10BoosterEachType()
    {
        var res = DataAccountPlayer.PlayerResourceData;
        res.ChangeBoosterCount(BoosterType.booster1, 10);
        res.ChangeBoosterCount(BoosterType.booster2, 10);
        res.ChangeBoosterCount(BoosterType.booster3, 10);
        res.ChangeBoosterCount(BoosterType.booster4, 10);
        RefreshBoosterUi();
    }
    private void RefreshBoosterUi()
    {
        BoosterManager.Instance.RefreshBoosterButtons();
    }

    #endregion

    #region StreakUI
    private void OnMoveToDayStreak()
    {
        if (!int.TryParse(inputDayStreak.text, out int day))
        {
            GameDebug.LogWarning("Daily Streak: nhập số ngày hợp lệ.");
            return;
        }

        DailyStreakService.CheatSetStreakDay(day);
        GameDebug.Log($"Daily Streak cheat: sẽ trigger check-in ngày {day} sau khi win màn tiếp theo.");
    }

    private void OnFinishStreak()
    {
        DailyStreakService.CheatFinishStreak();
        GameDebug.Log("Daily Streak cheat: hoàn thành streak, trigger gift ở màn sau khi win.");
    }
    #endregion

    #region Marketing Video
    private void OnDisableGameUiForMarketing()
    {
        UIGameHeader.SetActive(false);
        UIGameBottomBar.SetActive(false);
    }

    private void OnActiveGameUiForMarketing()
    {
        UIGameHeader.SetActive(true);
        UIGameBottomBar.SetActive(true);
    }

    private void OnActiveHand()
    {
        GameDebug.Log("Active hand");
        hand.SetActive(true);
    }

    private void OnDeActiveHand()
    {
        GameDebug.Log("DEActive hand");
        hand.SetActive(false);
    }
    #endregion

    #region Resource
    private void OnAdd1000Gold()
    {
        DataAccountPlayer.PlayerResourceData.ChangeGoldValue(1000);
    }

    private void OnAdd1000Heart()
    {
        DataAccountPlayer.PlayerResourceData.ChangeHeartValue(1000);
    }

    private void OnAdd10SkipAds()
    {
        DataAccountPlayer.PlayerResourceData.SetSkipAdsValue(10);
    }

    private void OnActiveInfiniteHeart1h()
    {
        //DataAccountPlayer.PlayerResourceData.SetInfiniteHeart(true, 3600);
    }

    #endregion

    #region Ads

    private void OnShowInter()
    {
        AdsManager.Instance.ShowInter();
    }

    private void OnShowBanner()
    {
        AdsManager.Instance.ShowBanner();
    }

    private void OnRemoveAds()
    {
        DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);
    }

    private void StartUpdateTimeCoroutine()
    {
        StopUpdateTimeCoroutine();
        if (gameObject.activeInHierarchy)
        {
            _updateTimeCoroutine = StartCoroutine(UpdateTimeRoutine());
        }
    }

    private void StopUpdateTimeCoroutine()
    {
        if (_updateTimeCoroutine != null)
        {
            StopCoroutine(_updateTimeCoroutine);
            _updateTimeCoroutine = null;
        }
    }

    private IEnumerator UpdateTimeRoutine()
    {
        var wait = new WaitForSecondsRealtime(0.5f);

        while (panelAdmin != null && panelAdmin.gameObject.activeInHierarchy)
        {
            UpdateTimeText();
            yield return wait;
        }

        _updateTimeCoroutine = null;
    }

    private void UpdateTimeText()
    {
        if (timeToNextInter != null && GameManager.Instance != null)
        {
            timeToNextInter.text = $"Time to next inter: {GameManager.Instance.timeCountAds:F1}s";
        }
    }

    #endregion
}
