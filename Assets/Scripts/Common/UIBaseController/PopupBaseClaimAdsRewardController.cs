using Base.Core.Sound;
using JinGroup.Base.LoadData;
using UnityEngine;
using UnityEngine.UI;

namespace JinGroup.Common.UIBaseController
{

    public class PopupBaseClaimAdsRewardController : PopupBaseRewardController
    {
        [SerializeField] private Button claim;
        [SerializeField] private Button claimAds;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void InitData()
        {
            base.InitData();
            listBundlePackData = LoadResourceController.Instance.DataRewardController().ContentContent.popupLevelUp;
            CreateReward();
        }


        protected override void ListenerButton()
        {
            base.ListenerButton();
            claim.onClick.AddListener(ClaimButton);
            claimAds.onClick.AddListener(ClaimAdsButton);
        }

        protected virtual void ClaimButton()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ClickButton);
        }

        protected virtual void ClaimAdsButton()
        {
            SoundManager.Instance.PlayOneShot(SoundType.ClickButton);
        }
    }
}