using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections;
using TMPro;
using UnityEngine;

public class PopupBundleStartePack : PopupBundleBaseController
{
    [SerializeField] private TextMeshProUGUI textLimitPurchase;
    [SerializeField] private TextMeshProUGUI textTimeCountDown;

    private int limitPurchase;
    private int levelUnlock;
    private int hourActive;
    private Coroutine _countdownCo;
    private bool _configLoaded;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        EnsureConfigLoaded();
        UpdateInforPurchase();

        if (!IsStarterPackCurrentlyAvailable())
        {
            ForceClosePopup();
            return;
        }

        StartTimeCountdown();
    }

    protected override void OnDisable()
    {
        StopTimeCountdown();
        base.OnDisable();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
    }

    protected override void InitData()
    {
        EnsureConfigLoaded();
        if (bundleData != null && bundleData.Count > 0)
            base.InitData();
    }

    private void EnsureConfigLoaded()
    {
        if (_configLoaded && bundleData != null && bundleData.Count > 0)
            return;

        var data = LoadResourceController.Instance.DataBundleController();
        bundleData = data.ContentContent.specialBundle;
        if (bundleData == null || bundleData.Count == 0)
            return;

        limitPurchase = bundleData[0].limitPurchase;
        levelUnlock = bundleData[0].levelUnlock;
        hourActive = bundleData[0].hourActive;
        _configLoaded = true;
    }

    private bool IsStarterPackCurrentlyAvailable()
    {
        var currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        return DataAccountPlayer.PlayerShopData.IsStarterPackActive(
            levelUnlock, hourActive, limitPurchase, currentLv);
    }

    public override void OnProcessPurchase()
    {
        base.OnProcessPurchase();
        DataAccountPlayer.PlayerShopData.UpdatePurchaseCountStarterPack();
        UpdateInforPurchase();
        JinGroup.Common.HomeUIManager.Instance?.RefreshStarterPackButton();
    }

    private void UpdateInforPurchase()
    {
        var numberCurrentPurchase = DataAccountPlayer.PlayerShopData.starterPackPurchase;
        if (textLimitPurchase != null)
            textLimitPurchase.text = (limitPurchase - numberCurrentPurchase) + "/" + limitPurchase + " Avaiable";

        if (limitPurchase > 0 && numberCurrentPurchase >= limitPurchase)
            ForceClosePopup();
    }

    private void StartTimeCountdown()
    {
        StopTimeCountdown();

        if (hourActive <= 0)
        {
            if (textTimeCountDown != null)
                textTimeCountDown.gameObject.SetActive(false);
            return;
        }

        if (textTimeCountDown != null)
            textTimeCountDown.gameObject.SetActive(true);

        _countdownCo = StartCoroutine(CoTimeCountdown());
    }

    private void StopTimeCountdown()
    {
        if (_countdownCo == null)
            return;
        StopCoroutine(_countdownCo);
        _countdownCo = null;
    }

    private IEnumerator CoTimeCountdown()
    {
        while (true)
        {
            var remaining = DataAccountPlayer.PlayerShopData.GetStarterPackRemaining(hourActive);

            if (remaining.TotalSeconds <= 0)
            {
                if (textTimeCountDown != null)
                    textTimeCountDown.text = "00:00:00";

                JinGroup.Common.HomeUIManager.Instance?.RefreshStarterPackButton();
                ForceClosePopup();
                yield break;
            }

            if (textTimeCountDown != null)
            {
                textTimeCountDown.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    remaining.Days * 24 + remaining.Hours,
                    remaining.Minutes,
                    remaining.Seconds);
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    /// <summary>Đóng popup bỏ qua close-block (dùng khi hết giờ / hết lượt mua).</summary>
    private void ForceClosePopup()
    {
        StopTimeCountdown();
        if (!gameObject.activeInHierarchy && !gameObject.activeSelf)
            return;

        if (PopupManager.Instance != null)
            PopupManager.Instance.CloseCurrentPopup();
        gameObject.SetActive(false);
    }
}
