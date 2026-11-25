using DataAccount;
using JinGroup.Controller.Feature;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class BoosterButtonController : MonoBehaviour
{
    private const string UnlockLevelFormat = "Lv.{0}";

    [SerializeField] private TextMeshProUGUI unlockTxt;
    [SerializeField] private TextMeshProUGUI valueBoosterTxt;
    [SerializeField] private TextMeshProUGUI priceTxt;
    [SerializeField] private GameObject valueBooster;

    [SerializeField] private GameObject getMore;
    [SerializeField] private GameObject priceHolder;
    [SerializeField] private Image boosterIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button boosterBtn;
    [SerializeField] private ScaleEffectSquashAndStretch effect;

    [Header("Receive Reward")]
    [SerializeField] private MMF_Player _myPlayer;

    private int currentValueBooster;
    private ProcessBoosterData _data;
    private bool _isUnlocked;
    private BoosterManager _boosterManager;
    private int _price;
    private GameConfig _gameConfig;
    public BoosterType BoosterType => _data.boosterType;
    private bool _isBoosterEmpty;
    private bool _isReceivingBooster;
    public bool IsReceivingBooster => _isReceivingBooster;

    public void SetReceivingBooster(bool isReceiving)
    {
        _isReceivingBooster = isReceiving;
    }

    private void OnDisable()
    {
        _isReceivingBooster = false;
    }

    private void Awake()
    {
        boosterBtn.onClick.AddListener(OnBoosterUsingClicked);
    }

    public void Init(BoosterManager boosterManager, ProcessBoosterData data, int currentLevel, GameConfig gameConfig)
    {
        _boosterManager = boosterManager;
        _data = data;
        _gameConfig = gameConfig;
        boosterIcon.sprite = _data.imgIconBooster;
        SetStatus(currentLevel);
        _price = _data.price * data.mutipleValueGold;
        priceTxt.text = _price.ToString();
    }

    public void PlayValueGainEffect(int remainingMoves)
    {
        if (_myPlayer == null) return;

        var floatingText = _myPlayer.GetFeedbackOfType<MMF_FloatingText>();
        if (floatingText == null) return;

        floatingText.Value = remainingMoves.ToString();
        floatingText.PositionMode = MMF_FloatingText.PositionModes.PlayPosition;

        _myPlayer.PlayFeedbacks(transform.position, 0);
    }

    public void SetStatus(int currentLevel)
    {
        int amount = DataAccountPlayer.PlayerResourceData.GetBoosterCount(_data.boosterType);
        currentValueBooster = Mathf.Max(0, amount);
        _isUnlocked = DataAccountPlayer.PlayerPointProcessData.cheatUnlockAllBoosters || currentLevel >= _data.level;
        RefreshVisuals();
    }

    public void PlayReceiveBoosterEffect(int addedAmount)
    {
        _isReceivingBooster = false;

        if (!_isUnlocked)
        {
            int currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
            _isUnlocked = DataAccountPlayer.PlayerPointProcessData.cheatUnlockAllBoosters || currentLevel >= _data.level;
        }

        if (!_isUnlocked || addedAmount <= 0)
            return;

        int toValue = DataAccountPlayer.PlayerResourceData.GetBoosterCount(_data.boosterType);

        currentValueBooster = toValue;
        _isBoosterEmpty = toValue <= 0;

        // Cập nhật ngay lập tức giao diện và giá trị số khi booster bay xuống chạm đích
        RefreshVisuals();

        if (effect != null)
            effect.Play();

        PlayValueGainEffect(toValue);
    }

    private void RefreshVisuals()
    {
        bool locked = !_isUnlocked;

        if (locked)
        {
            lockIcon.gameObject.SetActive(true);
            unlockTxt.gameObject.SetActive(true);
            unlockTxt.text = string.Format(UnlockLevelFormat, _data.level);
            boosterIcon.gameObject.SetActive(false);

            _isBoosterEmpty = false;
            valueBooster.SetActive(false);
            valueBoosterTxt.gameObject.SetActive(false);
            return;
        }

        lockIcon.gameObject.SetActive(false);
        unlockTxt.gameObject.SetActive(false);
        boosterIcon.gameObject.SetActive(true);

        _isBoosterEmpty = currentValueBooster <= 0;
        bool hasStock = !_isBoosterEmpty;
 
        valueBooster.SetActive(hasStock);
        var isIAA = _gameConfig != null ? !_gameConfig.usingResourceInGame : true;
        var showPrice = _gameConfig != null ? _gameConfig.showPriceUnderBoosterButton : false;

        priceHolder.SetActive(!hasStock && !isIAA && showPrice);
        getMore.SetActive(!hasStock);

        valueBoosterTxt.gameObject.SetActive(hasStock);

        if (hasStock)
            valueBoosterTxt.text = currentValueBooster.ToString();
    }

    private void OnBoosterUsingClicked()
    {
        // Chặn bấm logic trong quá trình booster đang bay xuống cộng số lượng (vẫn giữ interactable)
        if (_isReceivingBooster || (_boosterManager != null && _boosterManager.IsAnyBoosterReceiving))
            return;

        if (!_isUnlocked)
        {
            ToastManager.Instance.Show("Unlock At Level : " + _data.level);
            return;
        }

        _boosterManager.UseBooster(_data.boosterType, _isBoosterEmpty);
    }
}
