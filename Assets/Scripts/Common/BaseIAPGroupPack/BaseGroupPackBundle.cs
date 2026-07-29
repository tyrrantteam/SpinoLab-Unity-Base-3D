using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.UIBaseController
{
    public class BaseGroupPackBundle : MonoBehaviour
    {
        public int idPack;
        [SerializeField] private Button purchaseBtn;
        [SerializeField] private Text nameTxt;
        [SerializeField] private Text priceTxt;
        [SerializeField] private Text valueTxt;
        [HideInInspector] public List<BundleReward> listBundlePackData;
        [SerializeField] private Image iconPreview;

        protected virtual void Awake()
        {
            ListenerButton();
        }

        protected virtual void ListenerButton()
        {
            purchaseBtn.onClick.AddListener(OnClickPurchase);
        }

        public virtual void InitData()
        {
            nameTxt.text = listBundlePackData[idPack].name;
            priceTxt.text = listBundlePackData[idPack].price.ToString() + "$";
            valueTxt.text = listBundlePackData[idPack].value.ToString();
        }

        protected virtual void OnClickPurchase()
        {
            ClaimReward();
        }

        public void ClaimReward()
        {
            DataResourceHelper.ClaimReward(listBundlePackData);

            List<PopupReward> listReward = new List<PopupReward>();

            for (int i = 0; i < listBundlePackData.Count; i++)
            {
                PopupReward popupReward = new PopupReward();
                popupReward.id = listBundlePackData[i].id;
                popupReward.value = listBundlePackData[i].value;
                popupReward.typeResources = listBundlePackData[i].typeResources;
                listReward.Add(popupReward);
            }
            PopupManager.Instance.ModuleShowReward().OpenPopup(listReward);
        }
    }
}