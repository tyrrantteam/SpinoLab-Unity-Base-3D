using DataAccount;
using JinGroup.Common.ResourcesHeader;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartResourcesHeader : BaseResourcesHeader
{
    [SerializeField] private GameObject iconHeartInfinite;
    private Coroutine _countdownCoroutine;

    protected override void Awake()
    {
        base.Awake();
        this.RegisterListener(EventID.UpdateHeart, (sender, param) => UpdateValue());
    }

    private void OnEnable()
    {
        DataAccountPlayer.PlayerResourceData.UpdateHeartRecovery();
        UpdateValue();
        UpdateCountdownDisplay();
        StartCountdown();
    }

    private void OnDisable()
    {
        StopCountdown();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            DataAccountPlayer.PlayerResourceData.UpdateHeartRecovery();
            UpdateValue();
            UpdateCountdownDisplay();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            DataAccountPlayer.PlayerResourceData.UpdateHeartRecovery();
            UpdateValue();
            UpdateCountdownDisplay();
        }
    }

    protected override void UpdateValue()
    {
        var resourceData = DataAccountPlayer.PlayerResourceData;
        if (resourceData != null)
        {
            valueInformation = resourceData.heart;
            ChangeValueWithAnimation();
        }
        UpdateCountdownDisplay();
    }

    private void StartCountdown()
    {
        StopCountdown();
        if (gameObject.activeInHierarchy)
        {
            _countdownCoroutine = StartCoroutine(CountdownRoutine());
        }
    }

    private void StopCountdown()
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }
    }

    private IEnumerator CountdownRoutine()
    {
        var waitOneSecond = new WaitForSecondsRealtime(1f);
        while (true)
        {
            yield return waitOneSecond;
            UpdateCountdownDisplay();
        }
    }

    private void UpdateCountdownDisplay()
    {
        if (timeCountTxt == null) return;

        var resourceData = DataAccountPlayer.PlayerResourceData;
        if (resourceData == null) return;

        bool isInfinite = resourceData.IsInfiniteHeartActive();

        // 1: Bật/tắt icon vô hạn tim
        if (iconHeartInfinite != null && iconHeartInfinite.activeSelf != isInfinite)
        {
            iconHeartInfinite.SetActive(isInfinite);
        }

        // 2: Cập nhật timeCountTxt
        if (isInfinite)
        {
            timeCountTxt.text = FormatInfiniteHeartTime(resourceData.GetRemainingInfiniteHeartSeconds());
            return;
        }

        // Luồng hồi heart bình thường
        int limit = resourceData.GetHeartLimit();

        if (resourceData.heart >= limit)
        {
            timeCountTxt.text = "full";
            return;
        }

        resourceData.UpdateHeartRecovery();

        if (resourceData.heart >= limit)
        {
            timeCountTxt.text = "full";
            return;
        }

        long remaining = resourceData.GetRemainingRecoverySeconds();
        if (remaining >= 3600)
        {
            long hours = remaining / 3600;
            long minutes = (remaining % 3600) / 60;
            long seconds = remaining % 60;
            timeCountTxt.text = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
        else
        {
            long minutes = remaining / 60;
            long seconds = remaining % 60;
            timeCountTxt.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    /// <summary>
    /// Định dạng thời gian đếm ngược vô hạn tim:
    /// - 00days : 00h : 00m (khi còn ngày)
    /// - 00h : 00m : 00s (khi ngày = 0, còn giờ)
    /// - 00m : 00s (khi ngày = 0, giờ = 0)
    /// </summary>
    private string FormatInfiniteHeartTime(long remainingSeconds)
    {
        if (remainingSeconds <= 0) return "00m : 00s";

        long days = remainingSeconds / 86400;
        long hours = (remainingSeconds % 86400) / 3600;
        long minutes = (remainingSeconds % 3600) / 60;
        long seconds = remainingSeconds % 60;

        if (days > 0)
        {
            return $"{days:D2}days : {hours:D2}h : {minutes:D2}m";
        }

        if (hours > 0)
        {
            return $"{hours:D2}h : {minutes:D2}m : {seconds:D2}s";
        }

        return $"{minutes:D2}m : {seconds:D2}s";
    }
}
