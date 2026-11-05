using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using JinGroup.Controller.Popup.InGame.Popup.ContinueFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupContinueController : PopupContinueBaseController
{
    protected override void OnClosePopup()
    {
        base.OnClosePopup();
        if (gameConfig.usingHeart)
        {
            PopupManager.Instance.ShowPopup<PopupAreYourSureGiveUpController>();
        }
        else
        {
            GameController.Instance.SetLoseGame();
        }
    }
}
