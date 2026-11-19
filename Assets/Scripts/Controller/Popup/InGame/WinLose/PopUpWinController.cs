using Base.Core;
using DataAccount;
using DG.Tweening;
using JinGroup.Common.UIBaseController;
using LitMotion;
using TMPro;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;
using Ease = LitMotion.Ease;

public class PopUpWinController : PopupBaseController
{
    private int _level;

    [SerializeField] private Button nextLevelBtn;
    [SerializeField] private Button nextProcess;

    //[SerializeField] private Button tryAgain;
    private                  bool                  checkClick;
    [SerializeField] private RewardGaugeController rewardGauge;
    [SerializeField] private CoinRewardFlyEffect    coinFlyEffect;
    [SerializeField] private GameObject            process;
    [SerializeField] private GameObject            textNextMechanic;
    [SerializeField] private TextMeshProUGUI                  levelPreview;
    [Header("Gold Text ")] [SerializeField]
    private DataWinReward winReward;

    [SerializeField] private Text         rewardText;
    [SerializeField] private float        duration;
    private                  int          lastGoldValue;
    private                  Tween        goldTextTween;
    private                  MotionHandle goldTextHandle;

    protected override void Awake()
    {
        base.Awake();
        checkClick        = false;
        _level            = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        levelPreview.text = $"Level {_level} passed";
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        rewardGauge.OnRewardResult += OnGetResult;
        rewardGauge.OnRewardChange += OnRewardChange;
        lastGoldValue              =  DataAccountPlayer.PlayerResourceData.gold;
        rewardText.text            =  GetRewardValue().ToString();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        rewardGauge.OnRewardResult -= OnGetResult;
        rewardGauge.OnRewardChange -= OnRewardChange;
    }

    protected override void ListenerButton()
    {
        nextLevelBtn.onClick.AddListener(OnClickNext);
        nextProcess.onClick.AddListener(OnNextProcess);
    }

    private void OnRewardChange(int reward)
    {
        var value = GetRewardValue() * reward;
        rewardText.text = value.ToString();
    }

    private void OnGetResult(int reward)
    {
        var value = GetRewardValue() * reward;
        DataAccountPlayer.PlayerResourceData.ChangeGoldValue(value);
    }

    private int GetRewardValue()
    {
        var rewardLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen % 20;
        if (rewardLevel >= 20)
        {
            rewardLevel = 0;
        }

        return winReward.ContentContent.winReward[rewardLevel].value;
    }

    private void OnNextProcess()
    {
        coinFlyEffect?.Play();
        rewardGauge.transform.DOScale(0, 0.5f).OnComplete(() =>
        {
            nextProcess.interactable = false;
            rewardText.gameObject.SetActive(false);
            process.gameObject.SetActive(true);
            textNextMechanic.gameObject.SetActive(true);
            process.transform.DOScale(0.8f, 0.5f);
            textNextMechanic.transform.DOScale(1, 0.5f);
            rewardGauge.gameObject.SetActive(false);
            Invoke("ActiveBtnNextDecor", 0.65f);
        });
    }

    private void ActiveBtnNextDecor()
    {
        nextLevelBtn.gameObject.SetActive(true);
    }

    private void OnTryAgain()
    {
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        OnClosePopup();
    }

    private void OnClickNext()
    {
        if (!checkClick)
        {
            checkClick = true;
            GameController.Instance.CheckLevel();
            SpecialOfferAdsService.OnLevelWon();
            GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        }
    }

    protected override void OnClosePopup()
    {
        base.OnClosePopup();
    }
}