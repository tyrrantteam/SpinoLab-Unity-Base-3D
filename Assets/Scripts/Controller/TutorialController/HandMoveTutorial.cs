using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class HandMoveTutorial : MonoBehaviour
{
    [Header("Points (UI hoặc 3D, kéo thả vào)")]
    [SerializeField] private GameObject pointA;
    [SerializeField] private GameObject pointB;

    [Header("Move settings")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.InOutSine;
    [SerializeField] private bool playOnStart = true;

    private RectTransform _rectTransform;
    private MotionHandle _handle;

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
    }

    private void Start()
    {
        if (playOnStart)
        {
            Init(pointA, pointB);
        }
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


    private void OnDisable()
    {
        _handle.TryCancel();
    }

    public void Init(GameObject a, GameObject b)
    {
        if (a == null || b == null)
        {
            return;
        }

        Init(a.transform, b.transform);
    }

    public void Init(Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            return;
        }

        if (!TryGetAnchoredPosition(a, out Vector2 posA))
        {
            return;
        }

        if (!TryGetAnchoredPosition(b, out Vector2 posB))
        {
            return;
        }

        Play(posA, posB);
    }

    public void Stop()
    {
        _handle.TryCancel();
    }

    private void Play(Vector2 a, Vector2 b)
    {
        _handle.TryCancel();
        _rectTransform.anchoredPosition = a;
        _handle = LMotion.Create(a, b, duration)
            .WithEase(ease)
            .WithLoops(-1, LoopType.Yoyo)
            .BindToAnchoredPosition(_rectTransform);
    }

    private bool TryGetAnchoredPosition(Transform target, out Vector2 anchoredPosition)
    {
        anchoredPosition = Vector2.zero;

        Vector3 worldPos;
        Camera sourceCamera;

        if (target is RectTransform targetRect)
        {
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);
            worldPos = (corners[0] + corners[2]) / 2f;
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
            anchoredPosition = worldPos;
            return true;
        }

        Camera handCamera = GetCanvasCamera(_rectTransform);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, handCamera, out anchoredPosition);
    }

    private static Camera GetCanvasCamera(RectTransform rt)
    {
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return Camera.main;
        }

        canvas = canvas.rootCanvas;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }
}
