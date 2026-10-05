using UnityEngine;

namespace Base.Core.Debug
{
    [CreateAssetMenu(fileName = "GameDebugSettings", menuName = "Base/Core/Game Debug Settings")]
    public class GameDebugSettings : ScriptableObject
    {
        public const string ResourcePath = "Data/GameDebugSettings";

        [Tooltip("When enabled, GameDebug.Log / LogWarning / LogError / LogException are suppressed.")]
        [Header("Active debug and Admin Tool")]
        public bool isProduction;

        public bool IsLoggingEnabled => !isProduction;
    }
}
