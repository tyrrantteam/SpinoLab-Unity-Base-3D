using Base.Core;
using Base.Core.Debug;
using DataAccount;
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
    public int defaultValue = 1;

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
    }

    private void Start()
    {
        panelAdmin.gameObject.SetActive(false);
        openAdmin.interactable = !GameDebug.IsProduction;
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
    }

    public void CloseAdmin()
    {
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
        res.ChangeBoosterCount(BoosterType.Hammer, 10);
        res.ChangeBoosterCount(BoosterType.Scizor, 10);
        res.ChangeBoosterCount(BoosterType.FingerGod, 10);
        res.ChangeBoosterCount(BoosterType.Beam, 10);
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
}
