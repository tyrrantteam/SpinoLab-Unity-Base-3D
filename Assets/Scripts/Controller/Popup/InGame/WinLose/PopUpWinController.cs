using Base.Core;
using DataAccount;
using DG.Tweening;
using JinGroup.Common.UIBaseController;
using TMPro;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private Text rewardText;

    // Đảm bảo mỗi lần thắng chỉ cộng vàng 1 lần và chỉ chạy Transition 1 lần
    private bool _rewardClaimed;
    private bool _isTransitioning;

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
        rewardText.text            =  GetRewardValue().ToString();

        _rewardClaimed           = false;
        _isTransitioning         = false;
        nextProcess.interactable = true;
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        rewardGauge.OnRewardResult -= OnGetResult;
        rewardGauge.OnRewardChange -= OnRewardChange;

        CancelInvoke(nameof(ActiveBtnNextDecor));
        rewardGauge.transform.DOKill();
        process.transform.DOKill();
        textNextMechanic.transform.DOKill();
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
        ClaimReward(reward);
    }

    /// <summary>
    /// Cộng vàng ngay lập tức (đúng 1 lần), hiệu ứng xu bay chỉ để hiển thị.
    /// </summary>
    private void ClaimReward(int multiplier)
    {
        if (_rewardClaimed) return;
        _rewardClaimed = true;

        nextProcess.interactable = false;

        var value = GetRewardValue() * multiplier;
        DataAccountPlayer.PlayerResourceData.ChangeGoldValue(value);

        coinFlyEffect?.Play();
        Transition();
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
        // Bỏ qua thanh nhân: dừng gauge và nhận x1 vàng gốc
        rewardGauge.StopWithoutResult();
        ClaimReward(1);
    }

    private void Transition()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;

        rewardGauge.transform.DOScale(0, 0.5f).OnComplete(() =>
        {
            nextProcess.interactable = false;
            rewardText.gameObject.SetActive(false);
            process.gameObject.SetActive(true);
            textNextMechanic.gameObject.SetActive(true);
            process.transform.DOScale(0.8f, 0.5f);
            textNextMechanic.transform.DOScale(1, 0.5f);
            rewardGauge.gameObject.SetActive(false);
            Invoke(nameof(ActiveBtnNextDecor), 0.65f);
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