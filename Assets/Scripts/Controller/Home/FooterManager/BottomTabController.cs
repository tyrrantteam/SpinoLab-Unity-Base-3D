using System;
using Base.Core.Debug;
using DataAccount;
using JinGroup.Common;
using UnityEngine;
using UnityEngine.UI;

public enum TabType
{
    Shop    = 0,
    Home    = 1,
    Setting = 2,
}

public class BottomTabController : SingletonMono<BottomTabController>
{
    [SerializeField] private TabButton[]       groupTabBtn;
    [SerializeField] private GameObject        bannerAds;
    [SerializeField] private DataAdsController dataAdsController;

    private void Awake()
    {
        CheckNoAds();
    }

    private void CheckNoAds()
    {
        bool isNoAdsPurchased = DataAccountPlayer.PlayerResourceData.isNoAdsPurchase;

        bannerAds.SetActive(!isNoAdsPurchased && !dataAdsController.UsingBannerAds);
    }

    private void OnEnable()
    {
        for (int i = 0; i < groupTabBtn.Length; i++)
        {
            groupTabBtn[i].GetComponent<Toggle>().isOn = i == 1;
        }
    }


    public void SetPanelActive(TabType index)
    {
        for (int i = 0; i < groupTabBtn.Length; i++)
        {
            if (groupTabBtn[i] != null && groupTabBtn[i].TabType == index)
            {
                var toggle = groupTabBtn[i].GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.isOn = true;
                }

                break;
            }
        }
    }
}