using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class DailyCheckinElements : MonoBehaviour
{
    [SerializeField] private Button claimBtbn;
    [SerializeField] private Text dayTxt;
    [SerializeField] private GameObject wasClaim;

    public ModuleRewardController ModuleRewardController;
    [HideInInspector]public List<PopupRewardDailyCheckin> listBundlePackData;
    [SerializeField] private Transform holderReward;
    private List<ItemData> _listitemData;

    private bool _canClaim;

    private void Awake()
    {
        claimBtbn.onClick.AddListener(ClaimReward);
    }

    public void InitData(List<PopupRewardDailyCheckin> data)
    {
        listBundlePackData = data;
        _listitemData = LoadResourceController.Instance.DataItemController().ContentContent.ListitemData;
        CreateReward();
        dayTxt.text = "Day " + data[0].day;
        CheckCanClaim(data[0].day);
    }

    private void CheckCanClaim(int day)
    {

        var checkTime = DataAccountPlayer.PlayerDailyCheckinData.IsrightTime();
        var checkDay = DataAccountPlayer.PlayerDailyCheckinData.listDayCheckin.Contains(day);
        var compareDay = DataAccountPlayer.PlayerDailyCheckinData.CheckDay(day);


        _canClaim = checkTime && !checkDay && compareDay;
        if (_canClaim)
        {
            claimBtbn.interactable = true;
        }
        else
        {
            claimBtbn.interactable = false;
        }
        var checkWasClaim = DataAccountPlayer.PlayerDailyCheckinData.listDayCheckin.Contains(day);
        wasClaim.gameObject.SetActive(checkWasClaim);
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
            itemReward.DisableLayer();
        }
    }

    public void ClaimReward()
    {
        List<PopupReward> listReward = new List<PopupReward>();

        for (int i = 0; i < listBundlePackData.Count; i++)
        {
            PopupReward popupReward = new PopupReward();
            popupReward.id = listBundlePackData[i].id;
            popupReward.value = listBundlePackData[i].value;
            popupReward.typeResources = listBundlePackData[i].typeResources;
            listReward.Add(popupReward);
        }

        DataAccountPlayer.PlayerDailyCheckinData.SaveTheDay(listBundlePackData[0].day);
        DataResourceHelper.ClaimRewardDaily(listBundlePackData);
        wasClaim.gameObject.SetActive(true);
        PopupManager.Instance.ModuleShowReward().OpenPopup(listReward);
        this.PostEvent(EventID.ClaimDailyCheckin);
    }
}
