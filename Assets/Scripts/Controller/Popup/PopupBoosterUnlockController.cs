using DataAccount;
using JinGroup.Common.UIBaseController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupBoosterUnlockController : PopupBaseController
{
    [Header("Views")] public BoosterType boosterType;

    public TMP_Text boosterName;
    public TMP_Text boosterDescription;

    public Image boosterIcon;
    
    public void InitData(ProcessBoosterData data)
    {
        boosterName.text        = data.nameBooster;
        boosterDescription.text = data.description;

        boosterIcon.sprite = data.imgMechanic;
        boosterType        = data.boosterType;
    }

    protected override void OnClosePopup()
    {
        GameController.Instance.PlaySpawnIconBoosterItem(boosterType, () =>
        {
            if (!BoosterManager.IsNull)
                BoosterManager.Instance.ApplyBoosterReward(boosterType, 1);
            else
                DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(boosterType, 1);

            SpecialOfferAdsService.OnBoosterUnlocked(boosterType);
            this.PostEvent(EventID.OnUnlockBooster, boosterType);
            SpecialOfferAdsService.TryShowPopup();
        });
        base.OnClosePopup();
      
    }
}