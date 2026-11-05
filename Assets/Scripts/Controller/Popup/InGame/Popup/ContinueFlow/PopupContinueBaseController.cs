using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Controller.Popup.InGame.Popup.ContinueFlow
{

    public class PopupContinueBaseController : PopupBaseController
    {
        [SerializeField] private Button continueAdsBtn;
        [SerializeField] private Button continueGoldBtn;
        [SerializeField] private TextMeshProUGUI goldCostText;
        [HideInInspector] public GameConfig gameConfig;
        private int _goldCost;

        protected override void InitData()
        {
            base.InitData();
            gameConfig = LoadResourceController.Instance.GameConfig();
            _goldCost = gameConfig.priceContinuePopup;
            goldCostText.text = _goldCost.ToString();
        }

        protected override void ListenerButton()
        {
            base.ListenerButton();
            continueAdsBtn.onClick.AddListener(OnClickAdsContinue);
            continueGoldBtn.onClick.AddListener(OnClickContinue);
        }

        private void OnClickAdsContinue()
        {
            AdsManager.Instance.ShowRewarded(() =>
            {
                base.OnClosePopup();
                this.PostEvent(EventID.ContinueGame);
            });
        }


        private void OnClickContinue()
        {
            var gold = DataAccountPlayer.PlayerResourceData.gold;
            //to do : check gold cost
            if (gold >= _goldCost)
            {
                DataAccountPlayer.PlayerResourceData.ChangeGoldValue(-_goldCost);
                base.OnClosePopup();
                this.PostEvent(EventID.ContinueGame);
            }
            else
            {
                // Show popup Shop to buy gold
                //todo sửa popup base tắt shop xong show lại popup này 
                base.OnClosePopup();
            }
        }
    }
}