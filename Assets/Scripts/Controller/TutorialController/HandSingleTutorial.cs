using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HandSingleTutorial : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
    }

    public void SetPositionOnCanvas(GameObject target)
    {
        if (target == null) return;
        gameObject.SetActive(true);
        SetPositionOnCanvas(target.transform);
    }

    void Update()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            gameObject.SetActive(false);
        }
    }

    public void SetPositionOnCanvas(Transform target)
    {
        if (target == null) return;

        Vector3 worldPos;
        Camera sourceCamera;

        if (target is RectTransform targetRect)
        {
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            worldPos = (corners[0] + corners[2]) / 2f; // center của rect

            sourceCamera = GetCanvasCamera(targetRect);
        }
        else
        {
            worldPos = target.position;
            sourceCamera = Camera.main;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(sourceCamera, worldPos);

        RectTransform parent = _rectTransform.parent as RectTransform;
        if (parent == null)
        {
            _rectTransform.position = worldPos;
            return;
        }

        Camera handCamera = GetCanvasCamera(_rectTransform);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, handCamera, out Vector2 localPoint))
        {
            return;
        }

        _rectTransform.anchoredPosition = localPoint;
    }

    private static Camera GetCanvasCamera(RectTransform rt)
    {
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        if (canvas == null) return Camera.main;

        canvas = canvas.rootCanvas;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;

        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }
}