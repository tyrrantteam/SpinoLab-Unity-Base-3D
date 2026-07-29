using Base.Core.Debug;
using Core.DesignPattern.Factory;
using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class TutorialManager : SingletonMono<TutorialManager>
{
    [SerializeField] private RectTransform parentTutorial;
    [SerializeField] private TutorialData _tutorialData;
    private int _currentLv;
    private int _maxStepTut;
    private int _currentStepTut;
    private bool _hasDeferredTutorial;

    [HideInInspector]
    public HandSingleTutorial handMoveTutorial;
    [HideInInspector]
    public HandMoveTutorial handSlideTutorial;

    protected override void Awake()
    {
        base.Awake();
        this.RegisterListener(EventID.FinishTutorialStep, (sender, param) => RegisterTut());

        _currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        var dataTut = LoadResourceController.Instance.LoadDataTutorial();
        CreateHandTut();
        _tutorialData = dataTut.GetTutorialDataByLevel(_currentLv);
        InitDataTut();
    }


    private void CreateHandTut()
    {
        var handSingle = FactoryDesignPattern.Instance.CreateObjectUI("TUTORIAL_HAND", parentTutorial);
        handMoveTutorial = handSingle.GetComponent<HandSingleTutorial>();
        handMoveTutorial.transform.parent = parentTutorial;
        handMoveTutorial.gameObject.SetActive(false);

        var handSlide = FactoryDesignPattern.Instance.CreateObjectUI("TUTORIAL_HAND_SLIDE", parentTutorial);
        handSlideTutorial = handSlide.GetComponent<HandMoveTutorial>();
        handSlideTutorial.transform.parent = parentTutorial;
        handSlideTutorial.gameObject.SetActive(false);
    }

    private void InitDataTut()
    {
        if (_tutorialData == null)
            return;

        _maxStepTut = _tutorialData.stepTut;
        _currentStepTut = 0;
        _hasDeferredTutorial = true;
    }

    public void TryShowDeferredTutorial()
    {
        if (!_hasDeferredTutorial || _tutorialData == null)
            return;

        _hasDeferredTutorial = false;
        PopupManager.Instance.ShowPopup<PopupTutorialController>();
    }

    private void RegisterTut()
    {
        _currentLv += 1;
        if(_currentLv > _maxStepTut)
        {
            return;
        }
    }
}
