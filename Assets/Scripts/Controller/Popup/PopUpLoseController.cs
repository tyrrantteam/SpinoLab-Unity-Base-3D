using Base.Core;
using JinGroup.Common.UIBaseController;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class PopUpLoseController : PopupBaseController
{
    //[SerializeField] private Button homeBtn;
    [SerializeField] private Button tryAgainBtn;
    [SerializeField] private GameObject headerDecor;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        Invoke("ActiveHeaderDecor", 0.75f);
        Invoke("ActiveBtnNextDecor", 0.75f);
    }

    private void ActiveHeaderDecor()
    {
        headerDecor.SetActive(true);
    }

    private void ActiveBtnNextDecor()
    {
        tryAgainBtn.gameObject.SetActive(true);
    }

    protected override void ListenerButton()
    {
        //homeBtn.onClick.AddListener(OnClickHome);
        tryAgainBtn.onClick.AddListener(OnTryAgain);
    }
    private void OnTryAgain()
    {
        //GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        AnlyticManager.Instance.LogEventLevelTryAgain();
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        OnClosePopup();
    }
}