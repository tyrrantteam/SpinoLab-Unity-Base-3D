using JinGroup.Common.Effect;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// Light left-right sway on <see cref="Transform.localPosition"/> (X axis) via LitMotion punch.
/// <see cref="Play"/> can be called repeatedly; position always returns to the base captured in <see cref="Awake"/> (or via <see cref="RefreshBasePosition"/>).
/// </summary>
public class HorizontalShake : EffectBase
{
    [SerializeField]                float strength   = 0.1f;
    [SerializeField]                float duration   = 0.35f;
    [SerializeField]                int   vibrato    = 10;
    [SerializeField, Range(0f, 1f)] float elasticity = 1f;
    
    Vector3      _baseLocalPosition;
    MotionHandle _handle;

    protected void Awake()
    { 
        CaptureBase();
    }
    
    /// <summary>Begin horizontal shake from the stored base local position.</summary>
    [Button]
    public override void Play()
    {
        _handle.TryCancel();
        RestoreBase();

        var punchOffset = new Vector3(strength, 0f, 0f);
        float dampingRatio = 1f - elasticity;

        _handle = LMotion.Punch.Create(_baseLocalPosition, punchOffset, duration)
                         .WithFrequency(vibrato)
                         .WithDampingRatio(dampingRatio)
                         .WithOnComplete(RestoreBase)
                         .BindToLocalPosition(transform);
    }

    /// <summary>Stops the shake and snaps back to the base local position.</summary>
    public override void Stop()
    {
        _handle.TryCancel();
        RestoreBase();
    }

    /// <summary>Re-capture base position after the object was moved intentionally (not while shaking).</summary>
    public void RefreshBasePosition()
    {
        CaptureBase();
    }

    void CaptureBase()
    {
        _baseLocalPosition = transform.localPosition;
    }

    void RestoreBase()
    {
        transform.localPosition = _baseLocalPosition;
    }
}