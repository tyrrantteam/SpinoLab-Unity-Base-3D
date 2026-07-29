using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBaseRewardController : PopupBaseController
{
    public ModuleRewardController ModuleRewardController;
    [HideInInspector] public List<PopupReward> listBundlePackData;
    [SerializeField] private Transform holderReward;
    private List<ItemData> _listitemData;


    protected override void Awake()
    {
        ListenerButton();
        InitData();
    }


    protected override void InitData()
    {
        base.InitData();
        _listitemData = LoadResourceController.Instance.DataItemController().ContentContent.ListitemData;
    }

    public void CreateReward()
    {
        for (int i = 0; i < listBundlePackData.Count; i++)
        {
            dataItem dataItem = new dataItem();
            var matchedItems = _listitemData.Find(item => item.typeResources == listBundlePackData[i].typeResources);

            dataItem.typeRarity = (TypeRarity)Enum.Parse(typeof(TypeRarity), matchedItems.typeRarity, true);
            dataItem.typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), listBundlePackData[i].typeResources, true);
            dataItem.value = listBundlePackData[i].value;

            var itemReward = Instantiate(ModuleRewardController, holderReward);
            itemReward.InitData(dataItem);
        }
    }

    public void ClaimReward()
    {
        DataResourceHelper.ClaimRewardPopup(listBundlePackData);
        PopupManager.Instance.ModuleShowReward().OpenPopup(listBundlePackData);
    }

}
