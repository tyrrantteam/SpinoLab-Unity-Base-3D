using Base.Core;
using DataAccount;
using JinGroup.Base.LoadData;
using System;

public class AdsManager : SingletonMono<AdsManager>
{
    private DataAdsController dataAdsController;
    private void Start()
    {
        dataAdsController = GameManager.Instance.DataAds;
    }

    public void ShowInter(Action onClosed = null)
    {
        if (dataAdsController.UsingInterAds && GameManager.Instance.canShowAds)
        {
            GameManager.instance.ReSetValueAdsIntern();
            //AdMobManager.Instance.ShowInterstitial(onClosed);
            //MaxManager.Instance.ShowInterAds("", onClosed);
        }
        else
        {
            onClosed?.Invoke();
        }
    }

    public void ShowRewarded(Action onRewardEarned)
    {
        var skipAds = DataAccountPlayer.PlayerResourceData.skipAds;
        DataAccountPlayer.PlayerDailyTashkData.UpdateTask(TypeTaskDaily.WatchAds);
        if (skipAds > 0)
        {
            DataAccountPlayer.PlayerResourceData.ChangeSkipAdsValue(-1);
            onRewardEarned?.Invoke();
            return;
        }

        if (dataAdsController.UsingRewardAds)
        {
            GameManager.instance.ReSetValueAdsIntern();
            //AdMobManager.Instance.ShowRewarded(onRewardEarned);
            //MaxManager.Instance.ShowRewardAds("", onRewardEarned);
        }
    }

    public void ShowAOA(Action onClosed = null)
    {
        if (dataAdsController.UsingAOA)
        {
            //AdMobManager.Instance.ShowAppOpenAd(onClosed);
        }
    }

    public void ShowBanner()
    {
        if (dataAdsController.UsingBannerAds)
        {
            //AdMobManager.Instance.ShowBanner();
            //MaxManager.Instance.ShowBanner();
        }

    }

    public void HideBanner()
    {
        if (dataAdsController.UsingBannerAds)
        {
            //AdMobManager.Instance.HideBanner();
        }
    }
}
