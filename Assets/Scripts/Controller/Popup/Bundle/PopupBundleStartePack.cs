using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common;
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
        EnsureConfigLoaded();
        base.OnEnable();
        UpdatePurchaseText();

        if (!IsStarterPackCurrentlyAvailable())
        {
            // Unity ignores SetActive(false) while the object is still being enabled,
            // so close on the next frame instead.
            StartCoroutine(CoCloseNextFrame());
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
        base.InitData();
    }

    private void EnsureConfigLoaded()
    {
        if (_configLoaded && HasBundleData)
            return;

        var data = LoadResourceController.Instance.DataBundleController();
        bundleData = data != null ? data.ContentContent?.specialBundle : null;
        if (!HasBundleData)
            return;

        limitPurchase = bundleData[0].limitPurchase;
        levelUnlock = bundleData[0].levelUnlock;
        hourActive = bundleData[0].hourActive;
        _configLoaded = true;
    }

    private bool IsStarterPackCurrentlyAvailable()
    {
        if (!_configLoaded)
            return false;

        var currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        return DataAccountPlayer.PlayerShopData.IsStarterPackActive(
            levelUnlock, hourActive, limitPurchase, currentLv);
    }

    protected override void OnPurchaseSucceeded()
    {
        // Count first so the limit is saved even if the reward popup is never claimed.
        DataAccountPlayer.PlayerShopData.UpdatePurchaseCountStarterPack();
        base.OnPurchaseSucceeded(); // shows the reward popup and closes this popup
        UpdatePurchaseText();
        RefreshHomeStarterPackButton();
    }

    private void UpdatePurchaseText()
    {
        if (textLimitPurchase == null)
            return;

        var numberCurrentPurchase = DataAccountPlayer.PlayerShopData.starterPackPurchase;
        textLimitPurchase.text = (limitPurchase - numberCurrentPurchase) + "/" + limitPurchase + " Available";
    }

    private static void RefreshHomeStarterPackButton()
    {
        if (!HomeUIManager.IsNull)
            HomeUIManager.Instance.RefreshStarterPackButton();
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

    private IEnumerator CoCloseNextFrame()
    {
        yield return null;
        ForceClosePopup();
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

                _countdownCo = null;
                RefreshHomeStarterPackButton();
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

    /// <summary>Đóng popup bỏ qua close-block (dùng khi hết giờ / không còn khả dụng).</summary>
    private void ForceClosePopup()
    {
        StopTimeCountdown();
        if (!gameObject.activeSelf)
            return;

        if (PopupManager.Instance != null)
            PopupManager.Instance.CloseCurrentPopup();
        gameObject.SetActive(false);
    }
}
