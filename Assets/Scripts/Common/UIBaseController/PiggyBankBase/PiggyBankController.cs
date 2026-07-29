using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using JinGroup.Module.Resources;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Controller.PiggyBank
{
    public class PiggyBankController : PopupBaseController
    {
        [SerializeField] private Button purchaseBtn;
        [SerializeField] private Text currentValueTxt;
        [SerializeField] private Text priceTxt;
        [SerializeField] private Image sliderValue;
        [SerializeField] private TypeResources typePiggy;

        [HideInInspector] public int maxValue;
        [HideInInspector] public int currentValue;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void ListenerButton()
        {
            base.ListenerButton();
        }

        protected override void InitData()
        {
            var piggyData = LoadResourceController.Instance.DataPiggyBankController();
            var piggyDataType = piggyData.ContentContent.PiggyData(typePiggy);
            maxValue = piggyDataType.max_value;

            currentValueTxt.text = currentValue + "/" + maxValue;
            sliderValue.fillAmount = (float)currentValue/(float)maxValue;
            priceTxt.text = piggyDataType.price.ToString() + "$";
            base.InitData();
        }

        protected virtual void OnClickPurchase()
        {

        }

    }
}