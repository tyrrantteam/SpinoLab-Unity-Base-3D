namespace JinGroup.Common.Effect
{
    public enum EffectType
    {
        None,
        Scale,
        Move,
        Shake,
        // Float — Deprecated: chuyển sang EffectTypeLoop.Float (FloatingEffectUI)
        Fade,
        ButtonScale,
        SpawnCircle,
        MoveToTarget,
        Punch,
        ScaleSquashAndStretch
    }
}
