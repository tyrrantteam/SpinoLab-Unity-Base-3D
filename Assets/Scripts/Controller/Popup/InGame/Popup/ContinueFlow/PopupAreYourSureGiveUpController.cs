using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using JinGroup.Controller.Popup.InGame.Popup.ContinueFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupAreYourSureGiveUpController : PopupContinueBaseController
{
    protected override void OnClosePopup()
    {
        base.OnClosePopup();
        GameController.Instance.SetLoseGame();
    }
}
