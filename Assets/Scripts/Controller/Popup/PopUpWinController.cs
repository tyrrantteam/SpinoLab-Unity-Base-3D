using Base.Core;
using DataAccount;
using JinGroup.Common.UIBaseController;
using UI.LoadingScene;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWinController : PopupBaseController
{
    private int _level;
    [SerializeField] private Button nextLevelBtn;
    //[SerializeField] private Button tryAgain;
    [SerializeField] private GameObject headerDecor;
    [SerializeField] private GameObject starDecor;
    private bool checkClick;

    protected override void Awake()
    {
        base.Awake();
        checkClick = false;
        _level = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
    }

    private void OnEnable()
    {
        Invoke("ActiveStarDecor", 0.5f);
        Invoke("ActiveHeaderDecor", 0.5f);
        Invoke("ActiveBtnNextDecor", 0.65f);
    }

    private void ActiveBtnNextDecor()
    {
        nextLevelBtn.gameObject.SetActive(true);
    }

    private void ActiveStarDecor()
    {
        starDecor.SetActive(true);
    }

    private void ActiveHeaderDecor()
    {
        headerDecor.SetActive(true);
    }

    protected override void ListenerButton()
    {
       // tryAgain.onClick.AddListener(OnTryAgain);
        nextLevelBtn.onClick.AddListener(OnClickNext);
    }
    private void OnTryAgain()
    {
        GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        OnClosePopup();
    }
    private void OnClickNext()
    {
        if (!checkClick)
        {
            checkClick = true;
            GameController.Instance.CheckLevel();
            SpecialOfferAdsService.OnLevelWon();
            GameManager.Instance.LoadScene(SceneName.GamePlayScreen);
        }
    }

    protected override void OnClosePopup()
    {
        base.OnClosePopup();
      
    }
}