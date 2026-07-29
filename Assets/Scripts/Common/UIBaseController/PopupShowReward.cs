using Base.Core.Sound;
using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupShowReward : SingletonMono<PopupShowReward>
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Transform holderReward;
    [SerializeField] private ModuleRewardController ModuleRewardController;
    [SerializeField] private List<ModuleRewardController> listReward = new List<ModuleRewardController>();
    private List<ItemData> _listitemData;
    private bool isInitialized = false;

    public event Action OnPopupClosed;

    protected override void Awake()
    {
        base.Awake();
        closeBtn.onClick.AddListener(Close);
        InitData();
    }


    private void InitData()
    {
        _listitemData = LoadResourceController.Instance.DataItemController().ContentContent.ListitemData;
    }

    public void OpenPopup(List<PopupReward> listBundlePackData)
    {
        gameObject.SetActive(true);

        if (!isInitialized)
        {
            StartCoroutine(RewardGenerator(listBundlePackData));
            isInitialized = true;
        }
        else
        {
           StartCoroutine(UpdateRewards(listBundlePackData));
        }
    }

    private IEnumerator RewardGenerator(List<PopupReward> listBundlePackData)
    {
        foreach (var bundle in listBundlePackData)
        {
            SoundManager.Instance.PlaySound(SoundType.RewardShowItem);

            var reward = Instantiate(ModuleRewardController, holderReward);
            dataItem dataItem = CreateDataItem(bundle);

            reward.InitData(dataItem);
            listReward.Add(reward);
            
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator UpdateRewards(List<PopupReward> listBundlePackData)
    {
        for (int i = 0; i < listBundlePackData.Count; ++i)
        {
            if (i < listReward.Count)
            {
                SoundManager.Instance.PlaySound(SoundType.RewardShowItem);
                var dataItem = CreateDataItem(listBundlePackData[i]);
                listReward[i].InitData(dataItem);
                listReward[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                SoundManager.Instance.PlaySound(SoundType.RewardShowItem);
                var reward = Instantiate(ModuleRewardController, holderReward);
                var dataItem = CreateDataItem(listBundlePackData[i]);
                reward.InitData(dataItem);
                listReward.Add(reward);
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    private dataItem CreateDataItem(PopupReward popupReward)
    {
        var matchedItem = _listitemData.Find(item => item.typeResources == popupReward.typeResources);
        return new dataItem
        {
            typeRarity = (TypeRarity)Enum.Parse(typeof(TypeRarity), matchedItem.typeRarity, true),
            typeReward = (TypeResources)Enum.Parse(typeof(TypeResources), popupReward.typeResources, true),
            value = popupReward.value
        };
    }

    private void Close()
    {
        PopupManager.Instance.CloseCurrentPopup();
        gameObject.SetActive(false);
        for(int i = 0; i < listReward.Count; i++)
        {
            listReward[i].gameObject.SetActive(false);
        }

    }
}