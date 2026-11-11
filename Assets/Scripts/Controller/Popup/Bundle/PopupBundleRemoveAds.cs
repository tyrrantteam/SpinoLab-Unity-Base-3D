using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common;
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
        bundleData = data != null ? data.ContentContent?.noAdsBundle1 : null;
        base.InitData();
    }

    protected override void OnPurchaseSucceeded()
    {
        base.OnPurchaseSucceeded();
        DataAccountPlayer.PlayerResourceData.ChangeNoAdsStatus(true);

        if (!HomeUIManager.IsNull)
            HomeUIManager.Instance.RefreshBundleButtons();
    }
}
