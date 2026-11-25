using DataAccount;
using JinGroup.Base.LoadData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckySpinProgressionBarController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueTxt;
    [SerializeField] private Image fillImage;

    private void Awake()
    {
        this.RegisterListener(EventID.luckySpinUpdate, (sender, param) => UpdateProgressionBar());
    }

    private void Start()
    {
        UpdateProgressionBar();
    }

    private void UpdateProgressionBar()
    {
        var currentValue = DataAccountPlayer.PlayerLuckySpinData.currentLevelProcess;
        var dataLuckySpinController = LoadResourceController.Instance.DataLuckySpinController();
        var mileStone = dataLuckySpinController.MileStoneSpin;

        if (currentValue > mileStone)
        {
            currentValue = mileStone;
        }

        valueTxt.text = $"{currentValue}/{mileStone}";
        fillImage.fillAmount = (float)currentValue / mileStone;
    }

}
