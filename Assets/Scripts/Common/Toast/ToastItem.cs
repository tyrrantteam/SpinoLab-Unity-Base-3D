using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight data component on the Toast prefab.
/// Assign all references in the Inspector.
/// </summary>
public class ToastItem : MonoBehaviour
{
    [SerializeField] private TMP_Text      label;
    [SerializeField] private CanvasGroup   canvasGroup;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image         fillImage;

    public TMP_Text      Label         => label;
    public CanvasGroup   CanvasGroup   => canvasGroup;
    public RectTransform RectTransform => rectTransform;
    public Image         FillImage     => fillImage;
}
