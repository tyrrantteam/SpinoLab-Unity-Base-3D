using System;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace Base.Core.Debug
{
    /// <summary>
    /// Central debug gateway. Route all debug output through here instead of UnityEngine.Debug.
    /// Toggle <see cref="GameDebugSettings.isProduction"/> on the settings asset to silence logs in production builds.
    /// </summary>
    public static class GameDebug
    {
        private const string DefaultResourcePath = GameDebugSettings.ResourcePath;

        private static GameDebugSettings _settings;

        /// <summary>True when debug logs are allowed (not production).</summary>
        public static bool IsEnabled => GetSettings().IsLoggingEnabled;

        public static bool IsProduction => !IsEnabled;

        public static void Log(object message)
        {
            if (!IsEnabled) return;
            UnityDebug.Log(message);
        }

        public static void Log(object message, UnityEngine.Object context)
        {
            if (!IsEnabled) return;
            UnityDebug.Log(message, context);
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogFormat(format, args);
        }

        public static void LogFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogFormat(context, format, args);
        }

        public static void LogWarning(object message)
        {
            if (!IsEnabled) return;
            UnityDebug.LogWarning(message);
        }

        public static void LogWarning(object message, UnityEngine.Object context)
        {
            if (!IsEnabled) return;
            UnityDebug.LogWarning(message, context);
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogWarningFormat(format, args);
        }

        public static void LogWarningFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogWarningFormat(context, format, args);
        }

        public static void LogError(object message)
        {
            if (!IsEnabled) return;
            UnityDebug.LogError(message);
        }

        public static void LogError(object message, UnityEngine.Object context)
        {
            if (!IsEnabled) return;
            UnityDebug.LogError(message, context);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogErrorFormat(format, args);
        }

        public static void LogErrorFormat(UnityEngine.Object context, string format, params object[] args)
        {
            if (!IsEnabled) return;
            UnityDebug.LogErrorFormat(context, format, args);
        }

        public static void LogException(Exception exception)
        {
            if (!IsEnabled) return;
            UnityDebug.LogException(exception);
        }

        public static void LogException(Exception exception, UnityEngine.Object context)
        {
            if (!IsEnabled) return;
            UnityDebug.LogException(exception, context);
        }

        public static void SetSettings(GameDebugSettings settings)
        {
            _settings = settings;
        }

        private static GameDebugSettings GetSettings()
        {
            if (_settings != null)
                return _settings;

            _settings = Resources.Load<GameDebugSettings>(DefaultResourcePath);

            if (_settings != null)
                return _settings;

            _settings = ScriptableObject.CreateInstance<GameDebugSettings>();
#if !UNITY_EDITOR
            _settings.isProduction = true;
#endif
            return _settings;
        }
    }
}
