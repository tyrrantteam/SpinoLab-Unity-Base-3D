using DataAccount;
using JinGroup.Base.LoadData;
using JinGroup.Common.UIBaseController;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupTutorialController : PopupBaseController
{

    [SerializeField] private TutorialData _tutorialData;
    private int _currentLv;
    [SerializeField] private TextMeshProUGUI titleTxt;
    [SerializeField] private TextMeshProUGUI descriptionTxt;
    [SerializeField] private Image imagePreview;

    protected override void Awake()
    {
        base.Awake();
        _currentLv = DataAccountPlayer.PlayerPointProcessData.currentlevelShowScreen;
        var dataTut = LoadResourceController.Instance.LoadDataTutorial();
        _tutorialData = dataTut.GetTutorialDataByLevel(_currentLv);
        InitData();
    }

    protected override void InitData()
    {
        titleTxt.text = _tutorialData.nameTut;
        descriptionTxt.text = _tutorialData.descriptionTut;
        imagePreview.sprite = _tutorialData.previewTutorial;
    }
}
