using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using System;
using UnityEngine;

public class MoveWithCurve : EffectBase
{
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;

    [SerializeField] float duration = 1f;
    [SerializeField] float delay;

    [SerializeField] AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Added to the midpoint between A and B for a curved path. Leave zero for a straight line.")]
    [SerializeField] Vector3 arcMidOffset;

    [SerializeField] bool playOnEnable;

    [Tooltip("If Point A is null, movement starts from the current position.")]
    [SerializeField] bool allowStartFromCurrentPosition = true;

    public float Duration => duration;
    public float Delay => delay;
    public Vector3 ArcMidOffset => arcMidOffset;
    public AnimationCurve EaseCurve => easeCurve;
    public bool IsPlaying => _handle.IsActive();

    MotionHandle _handle;
    Action _onComplete;

    Vector3 _runtimeStart;
    Vector3 _runtimeEnd;
    bool _useRuntimePositions;
    float _runtimeDuration;
    bool _useRuntimeDuration;

    Transform _followEndTransform;
    Vector3 _fixedStart;
    bool _followEnd;

    protected override void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    public void Play(Action callback)
    {
        _onComplete = callback;
        Play();
    }

    public void Play(float playDuration, Action callback)
    {
        _runtimeDuration = playDuration;
        _useRuntimeDuration = true;
        Play(callback);
    }

    public override void Play()
    {
        _handle.TryCancel();
        float playDuration = _useRuntimeDuration ? _runtimeDuration : duration;
        _useRuntimeDuration = false;

        if (_followEnd && _followEndTransform != null)
        {
            PlayFollowingTarget(_fixedStart, _followEndTransform, playDuration);
            return;
        }

        if (!TryGetPath(out Vector3 start, out Vector3 end))
            return;

        bool useArc = arcMidOffset.sqrMagnitude > 0.000001f;
        if (!useArc)
        {
            _handle = LMotion.Create(start, end, playDuration)
                .WithDelay(delay)
                .WithEase(easeCurve)
                .WithOnComplete(InvokeComplete)
                .BindToPosition(transform);
            return;
        }

        Vector3 control = Vector3.Lerp(start, end, 0.5f) + arcMidOffset;
        _handle = LMotion.Create(0f, 1f, playDuration)
            .WithDelay(delay)
            .WithEase(easeCurve)
            .WithOnComplete(InvokeComplete)
            .Bind(t => transform.position = QuadraticBezier(start, control, end, t));
    }

    void PlayFollowingTarget(Vector3 start, Transform endTransform, float playDuration)
    {
        bool useArc = arcMidOffset.sqrMagnitude > 0.000001f;

        _handle = LMotion.Create(0f, 1f, playDuration)
            .WithDelay(delay)
            .WithEase(easeCurve)
            .WithOnComplete(() =>
            {
                if (endTransform != null)
                    transform.position = endTransform.position;
                InvokeComplete();
            })
            .Bind(t =>
            {
                if (endTransform == null)
                    return;

                Vector3 end = endTransform.position;

                if (!useArc)
                {
                    transform.position = Vector3.Lerp(start, end, t);
                    return;
                }

                Vector3 control = Vector3.Lerp(start, end, 0.5f) + arcMidOffset;
                transform.position = QuadraticBezier(start, control, end, t);
            });
    }

    public override void Stop()
    {
        _handle.TryCancel();
        _useRuntimePositions = false;
        _useRuntimeDuration = false;
        ClearFollowTarget();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClearFollowTarget();
    }

    public void SetPoints(Transform start, Transform end)
    {
        pointA = start;
        pointB = end;
        _useRuntimePositions = false;
        ClearFollowTarget();
    }

    public void SetPositions(Vector3 start, Vector3 end)
    {
        pointA = null;
        pointB = null;
        _runtimeStart = start;
        _runtimeEnd = end;
        _useRuntimePositions = true;
        arcMidOffset = Vector3.zero;
        ClearFollowTarget();
    }

    public void SetFollowTarget(Vector3 start, Transform end)
    {
        pointA = null;
        pointB = end;
        _fixedStart = start;
        _followEndTransform = end;
        _followEnd = end != null;
        _useRuntimePositions = false;
    }

    public void SetArcMidOffset(Vector3 offset)
    {
        arcMidOffset = offset;
    }

    void ClearFollowTarget()
    {
        _followEnd = false;
        _followEndTransform = null;
    }

    bool TryGetPath(out Vector3 start, out Vector3 end)
    {
        if (_useRuntimePositions)
        {
            start = _runtimeStart;
            end = _runtimeEnd;
            return true;
        }

        if (pointB == null)
        {
            start = default;
            end = default;
            return false;
        }

        start = pointA != null ? pointA.position : transform.position;
        if (!allowStartFromCurrentPosition && pointA == null)
        {
            end = default;
            return false;
        }

        end = pointB.position;
        return true;
    }

    void InvokeComplete()
    {
        Action temp = _onComplete;
        _onComplete = null;
        temp?.Invoke();
    }

    static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }
}
