using Base.Core.Debug;
using DataAccount;
using DG.Tweening;
using JinGroup.Base.LoadData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProcessMechanicController : MonoBehaviour
{
    private const string MechanicUnlockFormat = "New Mechanic Unlock at level {0}";
    private const string LevelUnlockText = "New Level unlock";

    [SerializeField] private Image fillLoadingProcess;
    [SerializeField] private Image mechanicUnlock;
    [SerializeField] private TextMeshProUGUI numberProcessPercentTxt;
    [SerializeField] private Image winImage;
    [SerializeField] private TextMeshProUGUI mechanicUnlockTxt;
    [SerializeField] private float percentAnimDuration = 0.6f;

    private int currentLevel;
    private DataProcessMechanic processMechanicData;
    private Tween _percentTween;

    private void Awake()
    {
        processMechanicData = LoadResourceController.Instance.DataProcessMechanic();

        if (fillLoadingProcess != null)
            fillLoadingProcess.type = Image.Type.Filled;
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnDisable()
    {
        _percentTween?.Kill();
        _percentTween = null;
    }

    public void RefreshUI()
    {
        _percentTween?.Kill();

        currentLevel = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;

        if (processMechanicData == null)
        {
            GameDebug.LogWarning("ProcessMechanicController: DataProcessMechanic is null.");
            return;
        }

        if (processMechanicData.HasRemainingMechanicUnlock(currentLevel))
            ShowMechanicProgress();
        else
            ShowAllMechanicsCompleted();
    }

    private void ShowMechanicProgress()
    {
        var nextMechanic = processMechanicData.GetDataProcesByCurrentLevel(currentLevel);
        int unlockLevel = nextMechanic.level;

        SetMechanicProgressActive(true);
        SetWinStateActive(false);

        if (nextMechanic.imgMechanic != null)
        {
            if (fillLoadingProcess != null)
                fillLoadingProcess.sprite = nextMechanic.imgMechanic;

            if (mechanicUnlock != null)
                mechanicUnlock.sprite = nextMechanic.imgMechanic;
        }

        if (mechanicUnlockTxt != null)
            mechanicUnlockTxt.text = string.Format(MechanicUnlockFormat, unlockLevel);

        int toPercent = processMechanicData.CalculatePercentByList(currentLevel);
        int fromPercent = GetFromPercent(toPercent);

        ApplyPercent(fromPercent);
        AnimatePercent(fromPercent, toPercent);
    }

    private void ShowAllMechanicsCompleted()
    {
        SetMechanicProgressActive(false);
        SetWinStateActive(true);

        if (mechanicUnlockTxt != null)
            mechanicUnlockTxt.text = LevelUnlockText;
    }

    private int GetFromPercent(int toPercent)
    {
        var playerData = DataAccountPlayer.PlayerPointProcessData;
        int savedPercent = playerData.lastMechanicProgressPercent;

        if (savedPercent > 0)
            return Mathf.Min(savedPercent, toPercent);

        int levelBeforeWin = Mathf.Max(1, currentLevel - 1);
        return processMechanicData.CalculatePercentByList(levelBeforeWin);
    }

    private void AnimatePercent(int fromPercent, int toPercent)
    {
        if (Mathf.Approximately(fromPercent, toPercent))
        {
            ApplyPercent(toPercent);
            SaveProgressPercent(toPercent);
            return;
        }

        float current = fromPercent;
        _percentTween = DOTween.To(
                () => current,
                value =>
                {
                    current = value;
                    ApplyPercent(Mathf.RoundToInt(current));
                },
                toPercent,
                percentAnimDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => SaveProgressPercent(toPercent));
    }

    private void ApplyPercent(int percent)
    {
        int clamped = Mathf.Clamp(percent, 0, 100);

        if (fillLoadingProcess != null)
            fillLoadingProcess.fillAmount = 1 - clamped / 100f;

        if (numberProcessPercentTxt != null)
            numberProcessPercentTxt.text = $"{clamped}%";
    }

    private static void SaveProgressPercent(int percent)
    {
        DataAccountPlayer.PlayerPointProcessData.SetLastMechanicProgressPercent(percent);
    }

    private void SetMechanicProgressActive(bool active)
    {
        fillLoadingProcess.gameObject.SetActive(active);
        mechanicUnlock.gameObject.SetActive(active);
        numberProcessPercentTxt.gameObject.SetActive(active);
    }

    private void SetWinStateActive(bool active)
    {
        if (winImage != null)
            winImage.gameObject.SetActive(active);
    }
}
