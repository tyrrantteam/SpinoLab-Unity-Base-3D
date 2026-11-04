using DataAccount;
using JinGroup.Common.UIBaseController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupProcessUnlockMechanicController : PopupBaseController
{
    [SerializeField] private TextMeshProUGUI TitleTxt;
    [SerializeField] private TextMeshProUGUI DescriptionTxt;
    [SerializeField] private Image previewMechImg;

    private int _listIndex = -1;

    public void Setup(DataProcessMechanic.ProcessMechanicData data, int listIndex)
    {
        _listIndex = listIndex;
        SetData(data);
    }

    public void SetData(DataProcessMechanic.ProcessMechanicData data)
    {
        if (TitleTxt != null)
            TitleTxt.text = data.nameMechanic;

        if (DescriptionTxt != null)
            DescriptionTxt.text = data.description ?? string.Empty;

        if (previewMechImg != null && data.imgMechanic != null)
        {
            previewMechImg.sprite = data.imgMechanic;
            previewMechImg.gameObject.SetActive(true);
        }
        else if (previewMechImg != null)
        {
            previewMechImg.gameObject.SetActive(false);
        }
    }

    public void SetData(string title, string description, Sprite previewImg)
    {
        if (TitleTxt != null)
            TitleTxt.text = title;

        if (DescriptionTxt != null)
            DescriptionTxt.text = description ?? string.Empty;

        if (previewMechImg != null)
        {
            previewMechImg.sprite = previewImg;
            previewMechImg.gameObject.SetActive(previewImg != null);
        }
    }

    protected override void OnClosePopup()
    {
        if (_listIndex >= 0)
            DataAccountPlayer.PlayerPointProcessData.MarkMechanicUnlockShown(_listIndex);

        _listIndex = -1;
        base.OnClosePopup();

        if (!GameController.IsNull)
            GameController.Instance.TryShowMechanicUnlockPopup();
    }
}
