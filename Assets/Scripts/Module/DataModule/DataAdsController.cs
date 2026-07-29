using DataAccount;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DataAds", menuName = "Data/Ads")]
public class DataAdsController : ScriptableObject
{
    public float timeShowInterAds1;
    public float timeShowInterAds2;
    public float timeShowInterAds3;

    public int breakpointLevelShowInterAds1;
    public int breakpointLevelShowInterAds2;
    public int breakpointLevelShowInterAds3;

    public bool UsingBannerAds;
    public bool UsingInterAds;

    public bool CanShowInterAds()
    {
        var level = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        if (level >= breakpointLevelShowInterAds1)
        {
            return true;
        }
        else if (level >= breakpointLevelShowInterAds2)
        {
            return true;
        }
        else if (level >= breakpointLevelShowInterAds3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float TimeShow()
    {
        var level = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        if (level >= breakpointLevelShowInterAds1)
        {
            return timeShowInterAds2;
        }
        else if (level >= breakpointLevelShowInterAds2)
        {
            return breakpointLevelShowInterAds3;
        }
        else if (level >= breakpointLevelShowInterAds3)
        {
            return timeShowInterAds3;
        }
        else
        {
            return timeShowInterAds1;
        }
    }
}
