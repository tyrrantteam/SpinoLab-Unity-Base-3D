using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

/// <summary>
/// Moves this transform from A to B using LitMotion.
/// <see cref="easeCurve"/> shapes how fast we move along the path over time.
/// If <see cref="arcMidOffset"/> is non-zero, the path is a quadratic Bézier (curved) through midpoint + offset; otherwise it is a straight segment.
/// </summary>
public class MoveWithCurve : EffectBase
{
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;

    [SerializeField] float duration = 1f;
    [SerializeField] float delay;

    [SerializeField] AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Added to the midpoint between A and B for a curved path. Leave zero for a straight line.")] [SerializeField]
    Vector3 arcMidOffset;

    [SerializeField] bool playOnEnable;

    [Tooltip("If Point A is null, movement starts from the current position.")] [SerializeField]
    bool allowStartFromCurrentPosition = true;

    MotionHandle _handle;


    /// <summary>Begin tween from A (or current position) to B.</summary>
    public override void Play()
    {
        _handle.TryCancel();

        if (pointB == null)
        {
            return;
        }

        Vector3 start = pointA != null ? pointA.position : transform.position;
        if (!allowStartFromCurrentPosition && pointA == null)
        {
            return;
        }

        Vector3 end = pointB.position;

        bool useArc = arcMidOffset.sqrMagnitude > 0.000001f;
        if (!useArc)
        {
            _handle = LMotion.Create(start, end, duration)
                             .WithDelay(delay)
                             .WithEase(easeCurve)
                             .BindToPosition(transform);
            return;
        }

        Vector3 control = Vector3.Lerp(start, end, 0.5f) + arcMidOffset;
        _handle = LMotion.Create(0f, 1f, duration)
                         .WithDelay(delay)
                         .WithEase(easeCurve)
                         .Bind(t => transform.position = QuadraticBezier(start, control, end, t));
    }

    /// <summary>Stops the current motion; transform stays where it is.</summary>
    public override void Stop()
    {
        _handle.TryCancel();
    }

    static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }
}