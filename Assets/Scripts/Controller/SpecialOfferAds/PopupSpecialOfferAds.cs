using DataAccount;
using JinGroup.Common.UIBaseController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupSpecialOfferAds : PopupBaseController
{
    public BoosterType typeBooster;

    [SerializeField] private Button claimBtn;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI descriptionTxt;

    private SpecialOfferAdsEntry _entry;
    private bool _claimed;

    public void Setup(SpecialOfferAdsEntry entry)
    {
        _entry = entry;
        _claimed = false;
        typeBooster = entry.boosterType;
        nameTxt.text = entry.name;
        descriptionTxt.text = entry.description;
    }

    protected override void ListenerButton()
    {
        base.ListenerButton();
        claimBtn.onClick.AddListener(ClaimBooster);
    }

    private void ClaimBooster()
    {
        if (_claimed)
            return;

        _claimed = true;
        claimBtn.interactable = false;

        int visualCount = Mathf.Clamp(_entry.value, 1, 5);
        GameController.Instance.PlaySpawnIconBoosterItem(typeBooster, () =>
        {
            if (!BoosterManager.IsNull)
                BoosterManager.Instance.ApplyBoosterReward(typeBooster, _entry.value);
            else
                DataAccountPlayer.PlayerResourceData.ChangeBoosterCount(typeBooster, _entry.value);

            SpecialOfferAdsService.OnOfferClaimed(typeBooster);
        }, visualCount);

        OnClosePopup();
    }

    protected override void OnClosePopup()
    {
        if (!_claimed)
            SpecialOfferAdsService.OnOfferDismissed();

        base.OnClosePopup();
    }
}
