using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JinGroup.Module.Resources;
using JinGroup.Base.LoadData;
namespace JinGroup.Module.Reward
{
    public class ModuleRewardController : MonoBehaviour
    {

        [SerializeField] private Image rewardIcon;
        [SerializeField] private Image backgroundIcon;
        [SerializeField] private Text valueTxt;

        public void InitData(dataItem dataItem)
        {
            rewardIcon.sprite = LoadResourceController.Instance.LoadItemIcon(dataItem.typeReward);
            backgroundIcon.sprite = LoadResourceController.Instance.LoadBackGroundIcon(dataItem.typeRarity);
            valueTxt.text = dataItem.value.ToString();
        }


        public void DisableLayer()
        {
            rewardIcon.raycastTarget = false;
            backgroundIcon.raycastTarget = false;
            valueTxt.raycastTarget = false;
        }

    }

    public struct dataItem
    {
        public TypeResources typeReward;
        public TypeRarity typeRarity;
        public int value;
        public int level;
    }
}