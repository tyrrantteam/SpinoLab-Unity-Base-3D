using Base.Core;
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
        if (dataAdsController.UsingRewardAds)
        {
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
