using JinGroup.Base.LoadData;
using UnityEngine;
using UnityEngine.UI;

public class HandUIFollowController : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private DataImageHand dataImageHand;
    [SerializeField] private Image handImage;
    public int handID;

    private RectTransform rectTransform;
    private RectTransform canvasRect;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = canvas.GetComponent<RectTransform>();
        dataImageHand = LoadResourceController.Instance.DataImageHand();
        SetHandID(1);
    }

    private void Update()
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            cam,
            out Vector2 localPos);

        rectTransform.localPosition = localPos;
    }
    private void LoadIDImage()
    {
        Debug.Log("a"+handID);
        var data = dataImageHand.GetDataByID(handID);
        if (data != null)
        {
            handImage.sprite = data.Image;
        }
    }
    public void SetHandID(int id)
    {
        handID = id;
        LoadIDImage();
    }
}