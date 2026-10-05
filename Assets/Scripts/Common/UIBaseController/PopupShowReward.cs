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
        if (closeBtn != null)
        {
            closeBtn.onClick.RemoveListener(Close);
            closeBtn.onClick.AddListener(Close);
        }
        InitData();
    }

    private void InitData()
    {
        if (LoadResourceController.Instance != null && LoadResourceController.Instance.DataItemController() != null)
        {
            var dataItemCtrl = LoadResourceController.Instance.DataItemController();
            if (dataItemCtrl.ContentContent != null)
            {
                _listitemData = dataItemCtrl.ContentContent.ListitemData;
            }
        }
    }

    public void OpenPopup(List<PopupReward> listBundlePackData)
    {
        if (listBundlePackData == null || listBundlePackData.Count == 0) return;

        transform.SetAsLastSibling();
        gameObject.SetActive(true);

        if (!gameObject.activeInHierarchy && transform.parent != null && !transform.parent.gameObject.activeInHierarchy)
        {
            transform.parent.gameObject.SetActive(true);
        }

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[PopupShowReward] Cannot start Coroutine because PopupShowReward or its parent is inactive in hierarchy!");
            return;
        }

        StopAllCoroutines();

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
        if (_listitemData == null)
        {
            InitData();
        }

        var matchedItem = _listitemData?.Find(item => item.typeResources == popupReward.typeResources);
        TypeRarity rarity = TypeRarity.normal;
        if (matchedItem != null && !string.IsNullOrEmpty(matchedItem.typeRarity))
        {
            Enum.TryParse(matchedItem.typeRarity, true, out rarity);
        }

        TypeResources resources = TypeResources.none;
        if (!string.IsNullOrEmpty(popupReward.typeResources))
        {
            Enum.TryParse(popupReward.typeResources, true, out resources);
        }

        return new dataItem
        {
            typeRarity = rarity,
            typeReward = resources,
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