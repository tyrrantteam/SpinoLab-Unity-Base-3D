using DataAccount;
using JinGroup.Controller.Feature;
using LitMotion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using Mono.Cecil.Cil;

public class BoosterButtonController : MonoBehaviour
{
    private const string UnlockLevelFormat = "Lv.{0}";

    [SerializeField] private TextMeshProUGUI unlockTxt;
    [SerializeField] private TextMeshProUGUI valueBoosterTxt;
    [SerializeField] private TextMeshProUGUI priceTxt;
    [SerializeField] private GameObject valueBooster;
    [SerializeField] private GameObject adsBooster;
    [SerializeField] private GameObject priceHolder;
    [SerializeField] private Image boosterIcon;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Button boosterBtn;
    [SerializeField] private ScaleEffectSquashAndStretch effect;

    [Header("Receive Reward")]
    [SerializeField] private float countUpDuration = 1.5f;
    [SerializeField] private Ease countUpEase = Ease.OutQuad;
    [SerializeField] private MMF_Player _myPlayer;

    private int currentValueBooster;
    private ProcessBoosterData _data;
    private bool _isUnlocked;
    private BoosterManager _boosterManager;
    private MotionHandle _countUpHandle;
    private bool _isCountingUp;
    private int _price;
    private GameConfig _gameConfig;
    public BoosterType BoosterType => _data.boosterType;
    private bool _isBoosterEmpty;

    private void Awake()
    {
        boosterBtn.onClick.AddListener(OnBoosterUsingClicked);
    }

    private void OnDisable()
    {
        _countUpHandle.TryCancel();
        _isCountingUp = false;
    }

    public void Init(BoosterManager boosterManager, ProcessBoosterData data, int currentLevel, GameConfig gameConfig)
    {
        _boosterManager = boosterManager;
        _data = data;
        _gameConfig = gameConfig;
        boosterIcon.sprite = _data.imgIconBooster;
        SetStatus(currentLevel);
        _price = _data.price;
        priceTxt.text = _data.price.ToString();
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
        if (_isCountingUp)
            return;

        int amount = DataAccountPlayer.PlayerResourceData.GetBoosterCount(_data.boosterType);
        currentValueBooster = Mathf.Max(0, amount);
        _isUnlocked = DataAccountPlayer.PlayerPointProcessData.cheatUnlockAllBoosters || currentLevel >= _data.level;
        RefreshVisuals();
    }

    public void PlayReceiveBoosterEffect(int addedAmount)
    {
        if (!_isUnlocked || addedAmount <= 0)
            return;

        int toValue = DataAccountPlayer.PlayerResourceData.GetBoosterCount(_data.boosterType);
        int fromValue = Mathf.Max(0, toValue - addedAmount);

        if (_countUpHandle.IsActive())
            _countUpHandle.TryCancel();

        PrepareValueUiForReceive(fromValue);
        PlayCountUp(fromValue, toValue);
    }

    private void PrepareValueUiForReceive(int fromValue)
    {
        lockIcon.gameObject.SetActive(false);
        unlockTxt.gameObject.SetActive(false);
        boosterIcon.gameObject.SetActive(true);

        valueBooster.SetActive(true);
        adsBooster.SetActive(false);
        valueBoosterTxt.gameObject.SetActive(true);
        valueBoosterTxt.text = fromValue.ToString();
        currentValueBooster = fromValue;
        _isBoosterEmpty = fromValue <= 0;
    }

    private void PlayCountUp(int fromValue, int toValue)
    {
        _isCountingUp = true;

        if (effect != null)
            effect.Play();

        if (fromValue >= toValue)
        {
            FinishCountUp(toValue);
            return;
        }

        _countUpHandle = LMotion.Create((float)fromValue, (float)toValue, countUpDuration)
            .WithEase(countUpEase)
            .WithOnComplete(() => FinishCountUp(toValue))
            .Bind(v =>
            {
                int display = Mathf.RoundToInt(v);
                if (display == currentValueBooster)
                    return;

                currentValueBooster = display;
                valueBoosterTxt.text = display.ToString();
            });

        PlayValueGainEffect(toValue);
    }

    private void FinishCountUp(int toValue)
    {
        currentValueBooster = toValue;
        valueBoosterTxt.text = toValue.ToString();
        _isBoosterEmpty = toValue <= 0;
        _isCountingUp = false;
        RefreshVisuals();
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
            adsBooster.SetActive(false);
            valueBoosterTxt.gameObject.SetActive(false);
            return;
        }

        lockIcon.gameObject.SetActive(false);
        unlockTxt.gameObject.SetActive(false);
        boosterIcon.gameObject.SetActive(true);

        _isBoosterEmpty = currentValueBooster <= 0;
        bool hasStock = !_isBoosterEmpty;

        valueBooster.SetActive(hasStock);
        var isIAA = _gameConfig.isIAAprod;

        adsBooster.SetActive(!hasStock && isIAA);
        priceHolder.SetActive(!hasStock && !isIAA);

        valueBoosterTxt.gameObject.SetActive(hasStock);

        if (hasStock)
            valueBoosterTxt.text = currentValueBooster.ToString();
    }

    private void OnBoosterUsingClicked()
    {
        if (!_isUnlocked)
        {
            ToastManager.Instance.Show("Unlock At Level : " + _data.level);
            return;
        }

        _boosterManager.UseBooster(_data.boosterType, _isBoosterEmpty);
    }
}
