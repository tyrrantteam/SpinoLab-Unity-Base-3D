using Base.Core;
using DataAccount;
using JinGroup.Common.UIBaseController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupAreYourSureController : PopupBaseController
{
    [SerializeField] private Button goHomeBtn;

    protected override void ListenerButton()
    {
        base.ListenerButton();
        goHomeBtn.onClick.AddListener(OnClickGoHome);
    }

    private void OnClickGoHome()
    {
        DataAccountPlayer.PlayerResourceData.ChangeHeartValue(-1);
        GameManager.Instance.LoadScene(UI.LoadingScene.SceneName.HomeScene);
    }
}
