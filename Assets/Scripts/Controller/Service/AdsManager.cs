using Base.Core;
using System;

public class AdsManager : SingletonMono<AdsManager>
{
    public void ShowInter(Action onClosed = null)
    {
        if (GameManager.Instance.canShowAds)
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

        //AdMobManager.Instance.ShowRewarded(onRewardEarned);
        //MaxManager.Instance.ShowRewardAds("", onRewardEarned);
    }

    public void ShowAOA(Action onClosed = null)
    {
        //AdMobManager.Instance.ShowAppOpenAd(onClosed);

    }

    public void ShowBanner()
    {
        //AdMobManager.Instance.ShowBanner();
        //MaxManager.Instance.ShowBanner();
    }

    public void HideBanner()
    {
        //AdMobManager.Instance.HideBanner();
    }
}
