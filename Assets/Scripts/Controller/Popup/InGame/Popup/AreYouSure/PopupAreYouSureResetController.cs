using Base.Core;
using DataAccount;
using JinGroup.Common.UIBaseController;
using UnityEngine;
using UnityEngine.UI;

public class PopupAreYouSureResetController : PopupBaseController
{
    [SerializeField] private Button restartGameBtn;

    protected override void ListenerButton()
    {
        base.ListenerButton();
        restartGameBtn.onClick.AddListener(OnClickGoHome);
    }

    private void OnClickGoHome()
    {
        DataAccountPlayer.PlayerResourceData.ChangeHeartValue(-1);
        GameManager.Instance.LoadScene(UI.LoadingScene.SceneName.GamePlayScreen);
    }
}
