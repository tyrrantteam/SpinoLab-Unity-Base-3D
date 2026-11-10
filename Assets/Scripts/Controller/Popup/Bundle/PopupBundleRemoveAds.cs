using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBundleRemoveAds : PopupBundleBaseController
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
    }

    protected override void InitData()
    {
        var data = LoadResourceController.Instance.DataBundleController();
        bundleData = data.ContentContent.noAdsBundle1;
        base.InitData();
    }
    
    public override void OnProcessPurchase()
    {
        base.OnProcessPurchase();
        DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);
    }
}
