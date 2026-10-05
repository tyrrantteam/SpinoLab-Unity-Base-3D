using DataAccount;
using JinGroup.Common.ResourcesHeader;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartResourcesHeader : BaseResourcesHeader
{
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
        valueInformation = DataAccountPlayer.PlayerResourceData.heart;
        ChangeValueWithAnimation();
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
}
