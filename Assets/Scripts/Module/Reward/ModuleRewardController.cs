using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JinGroup.Module.Resources;
using JinGroup.Base.LoadData;
using Base.Core.Debug;
using TMPro;

namespace JinGroup.Module.Reward
{
    public class ModuleRewardController : MonoBehaviour
    {
        public TypeResources typeResources;
        [SerializeField] protected Image rewardIcon;
        //[SerializeField] protected Image backgroundIcon;
        [SerializeField] protected TMP_Text valueTxt;

        public virtual void InitData(dataItem dataItem)
        {
            var type = dataItem.typeReward;
            typeResources = type;

            var icon = LoadResourceController.Instance.LoadItemIcon(type);
            if (icon != null)
            {
                rewardIcon.sprite = icon;
            }
            else
            {
                GameDebug.LogError($"[Reward] Không load được Sprite cho type = {type}");
            }

            string text = GetValueText(type, dataItem.value);
            bool showValue = text != null;
            if (showValue)
            {
                valueTxt.text = text;
            }

            // Always set active state so pooled/reused items don't keep a stale hidden/visible state
            if (valueTxt.gameObject.activeSelf != showValue)
            {
                valueTxt.gameObject.SetActive(showValue);
            }
        }

        /// <summary>Returns the value label for a reward, or null if the label should be hidden.</summary>
        private static string GetValueText(TypeResources type, int value)
        {
            if (value == 0) return null;

            switch (type)
            {
                // case TypeResources.noads:
                // case TypeResources.noads7d:
                // case TypeResources.noads24h:
                //     return null;
                case TypeResources.heart:
                    return value + "h";
                case TypeResources.booster1:
                case TypeResources.booster2:
                case TypeResources.booster3:
                    return "x" + value;
                default:
                    return null;
            }
        }
        
        public void DisableLayer()
        {
            rewardIcon.raycastTarget = false;
            //backgroundIcon.raycastTarget = false;
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