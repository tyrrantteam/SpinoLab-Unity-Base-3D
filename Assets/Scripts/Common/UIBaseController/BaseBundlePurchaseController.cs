using JinGroup.Base.LoadData;
using JinGroup.Module.Resources;
using JinGroup.Module.Reward;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.UIBaseController
{
    public class BaseBundlePurchaseController : MonoBehaviour
    {
        [SerializeField]  private Button purchaseBtn;
        [HideInInspector] public List<BundleReward> listBundlePackData;
        [SerializeField]  private Transform holderReward;
        public ModuleRewardController ModuleRewardController;
        private List<ItemData> _listitemData;

        protected virtual void Awake()
        {
            ListenerButton();
            InitData();
        }

        protected virtual void ListenerButton()
        {
            purchaseBtn.onClick.AddListener(OnClickPurchase);
        }

        protected virtual void InitData()
        {
            _listitemData = LoadResourceController.Instance.DataItemController().ContentContent.ListitemData;
        }

        protected virtual void OnClickPurchase()
        {
            ClaimReward();
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

                var itemReward = Instantiate(ModuleRewardController,holderReward);
                itemReward.InitData(dataItem);
            }
        }

        public void ClaimReward()
        {
            DataResourceHelper.ClaimReward(listBundlePackData);

            List<PopupReward> listReward = new List<PopupReward>();

            for (int i = 0;i < listBundlePackData.Count; i++)
            {
                PopupReward popupReward = new PopupReward();
                popupReward.id = listBundlePackData[i].id;
                popupReward.value = listBundlePackData[i].value;
                popupReward.typeResources = listBundlePackData[i].typeResources;
                listReward.Add(popupReward);
            }
            PopupManager.Instance.ModuleShowReward(). OpenPopup(listReward);
        }
    }
}