using Base.Core.Debug;
using Base.Core.Sound;
using DG.Tweening;
using JinGroup.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TabButton : MonoBehaviour
{
    [SerializeField] private TabType scene;
    public                   TabType TabType => scene;

    [Header("Move Active tab")] [SerializeField]
    private RectTransform endPoint;

    [SerializeField] private RectTransform startPoint;
    [SerializeField] private RectTransform activeButton;
    [SerializeField] private float         moveDuration;

    [Header("Icon")] [SerializeField] private RectTransform icon;
    [SerializeField]                  private RectTransform iconStartPoint;
    [SerializeField]                  private RectTransform iconEndPoint;

    [SerializeField] private float iconDuration;
    [SerializeField] private float imgScale;

    public UnityEvent<TabType> OnClickTabButton;

    public void PlayButtonSound()
    {
        SoundManager.Instance.PlaySound(SoundType.button_click);
    }

    [Button]
    public void OnToggleActive(bool active)
    {
        if (active)
        {
            activeButton.DOAnchorPos(endPoint.anchoredPosition, moveDuration);
            icon.DOScale(imgScale, moveDuration);
            icon.DOAnchorPos(iconEndPoint.anchoredPosition, moveDuration);
        }
        else
        {
            activeButton.DOAnchorPos(startPoint.anchoredPosition, moveDuration);
            icon.DOScale(1, moveDuration);
            icon.DOAnchorPos(iconStartPoint.anchoredPosition, moveDuration);
        }
    }
}