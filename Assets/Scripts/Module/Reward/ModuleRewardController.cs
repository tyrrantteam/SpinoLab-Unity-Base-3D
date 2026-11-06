using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JinGroup.Module.Resources;
using JinGroup.Base.LoadData;
using Base.Core.Debug;

namespace JinGroup.Module.Reward
{
    public class ModuleRewardController : MonoBehaviour
    {
        public TypeResources typeResources;
        [SerializeField] protected Image rewardIcon;
        //[SerializeField] protected Image backgroundIcon;
        [SerializeField] protected Text valueTxt;

        public virtual void InitData(dataItem dataItem)
        {
            typeResources = dataItem.typeReward;

            GameDebug.Log($"[Reward] TypeResources truyền vào LoadItemIcon = {dataItem.typeReward}");

            var icon = LoadResourceController.Instance.LoadItemIcon(dataItem.typeReward);
            if (icon != null)
            {
                rewardIcon.sprite = icon;
            }
            else
            {
                GameDebug.LogError($"[Reward] Không load được Sprite cho type = {dataItem.typeReward}");
            }
            if (dataItem.typeReward == TypeResources.noads7d || dataItem.typeReward == TypeResources.noads24h || dataItem.typeReward == TypeResources.noads)
            {
                valueTxt.gameObject.SetActive(false);
            }
            else if (dataItem.typeReward == TypeResources.heart)
            {
                valueTxt.text = dataItem.value+"h";
            }
            else
            {
                valueTxt.text = dataItem.value.ToString();
            }
            if (dataItem.value == 0) valueTxt.gameObject.SetActive(false);
        }

        public virtual void InitData(shopDataItem dataItem)
        {
            typeResources = dataItem.typeReward;
        }


        public void DisableLayer()
        {
            rewardIcon.raycastTarget = false;
            //backgroundIcon.raycastTarget = false;
            valueTxt.raycastTarget = false;
        }
    }
    public struct shopDataItem
    {
        public TypeResources typeReward;
        public TypeRarity typeRarity;
        public string value;
        public int level;
    }
    public struct dataItem
    {
        public TypeResources typeReward;
        public TypeRarity typeRarity;
        public int value;
        public int level;
    }
}