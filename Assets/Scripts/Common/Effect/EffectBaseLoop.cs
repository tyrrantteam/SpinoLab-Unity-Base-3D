using Sirenix.OdinInspector;
using LitMotion;
using UnityEngine;

namespace JinGroup.Common.Effect
{
    /// <summary>
    /// Base class cho các Loop Effect (breath, rotate, v.v.).
    /// Kế thừa <see cref="EffectBase"/> và bổ sung config vòng lặp + duration.
    /// </summary>
    public abstract class EffectBaseLoop : EffectBase
    {
        [Title("Loop Settings")]
        [LabelText("Infinite Loop")]
        [SerializeField] private bool infiniteLoop = true;

        [HideIf("infiniteLoop")]
        [LabelText("Loop Count")]
        [SerializeField] private int loopCount = 3;

        [LabelText("Duration (per cycle)")]
        [SerializeField] protected float duration = 1f;

        /// <summary>
        /// Số vòng lặp truyền vào LitMotion: -1 nếu infinite, ngược lại là loopCount.
        /// </summary>
        protected int LoopCount => infiniteLoop ? -1 : loopCount;
    }
}
