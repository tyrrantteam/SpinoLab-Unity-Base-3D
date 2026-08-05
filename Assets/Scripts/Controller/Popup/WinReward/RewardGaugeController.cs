using System;
using System.Collections;
using JinGroup.Common.UIBaseController;
using UnityEngine;
using UnityEngine.UI;

public class RewardGaugeController : MonoBehaviour
{
    [Header("Gauge")] [SerializeField]    private RectTransform needle;
    [SerializeField]                      private RectTransform gaugeBar;
    [SerializeField]                      private float         needleSpeed = 300f;
    [SerializeField]                      private float         maxAngle    = 45f;
    [Header("Segments")] [SerializeField] private Image[]       segmentImages;


    [Header("Result")] [SerializeField] private Button              btnStop;
    [SerializeField]                    private CoinRewardFlyEffect coinFlyEffect;
    private static readonly                     int[]               Multipliers = { 1, 2, 3, 2, 1 };

    private bool      _isRunning;
    private float     _direction = 1f;
    private Coroutine _needleRoutine;
    private int       _lastSegmentIndex;

    public event Action<int> OnRewardResult;
    public event Action<int> OnRewardChange;

    private void Awake()
    {
        btnStop.onClick.AddListener(OnTapStop);
    }

    protected void OnEnable()
    {
        StartGauge();
    }

    protected void OnDisable()
    {
        if (_needleRoutine != null)
            StopCoroutine(_needleRoutine);
    }

    private void StartGauge()
    {
        _isRunning              = true;
        _direction              = 1f;
        needle.anchoredPosition = new Vector2(0f, needle.anchoredPosition.y);
        _needleRoutine          = StartCoroutine(AnimateNeedle());
    }

    private IEnumerator AnimateNeedle()
    {
        float segmentAngle = (maxAngle * 2f) / 5f;

        while (_isRunning)
        {
            float currentZ = needle.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f;

            float nextZ = currentZ + _direction * needleSpeed * Time.deltaTime;

            if (nextZ >= maxAngle)
            {
                nextZ      = maxAngle;
                _direction = -1f;
            }
            else if (nextZ <= -maxAngle)
            {
                nextZ      = -maxAngle;
                _direction = 1f;
            }

            needle.localEulerAngles = new Vector3(0f, 0f, nextZ);

            UpdateValueDisplay(nextZ);

            yield return null;
        }
    }

    private void UpdateValueDisplay(float currentZ)
    {
        float segmentAngle = (maxAngle * 2f) / 5f;
        int segIndex = Mathf.Clamp(Mathf.FloorToInt((currentZ + maxAngle) / segmentAngle), 0, 4);

        if (segIndex != _lastSegmentIndex)
        {
            _lastSegmentIndex = segIndex;
            int multiplier = Multipliers[segIndex];
            OnRewardChange?.Invoke(multiplier);
        }
    }

    private void OnTapStop()
    {
        if (!_isRunning) return;
        _isRunning = false;

        float currentZ = needle.localEulerAngles.z;
        if (currentZ > 180f) currentZ -= 360f;

        float segmentAngle = (maxAngle * 2f) / 5f;
        int segmentIndex = Mathf.Clamp(Mathf.FloorToInt((currentZ + maxAngle) / segmentAngle), 0, 4);

        HighlightSegment(segmentIndex);

        int multiplier = Multipliers[segmentIndex];
        OnRewardResult?.Invoke(multiplier);
        coinFlyEffect?.Play();
    }

    private void HighlightSegment(int index)
    {
        for (int i = 0; i < segmentImages.Length; i++)
        {
            if (segmentImages[i] != null)
            {
                //todo: highlight
            }
        }
    }
}