using UnityEngine;
public static class Logger
{
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }
    public static LogLevel currentLogLevel = LogLevel.Info;
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message)
    {
        if (currentLogLevel >= LogLevel.Info)
        {
            Debug.Log($"[INFO] {message}");
        }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(object message, Object context)
    {
        if (currentLogLevel >= LogLevel.Info)
        {
            Debug.Log($"[INFO] {message}", context);
        }
    }
    public static void LogWarning(object message)
    {
        if (currentLogLevel >= LogLevel.Warning)
        {
            Debug.LogWarning($"[WARNING] {message}");
        }
    }
    public static void LogWarning(object message, Object context)
    {
        if (currentLogLevel >= LogLevel.Warning)
        {
            Debug.LogWarning($"[WARNING] {message}", context);
        }
    }
    public static void LogError(object message)
    {
        if (currentLogLevel >= LogLevel.Error)
        {
            Debug.LogError($"[ERROR] {message}");
        }
    }
    public static void LogError(object message, Object context)
    {
        if (currentLogLevel >= LogLevel.Error)
        {
            Debug.LogError($"[ERROR] {message}", context);
        }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogDebug(object message)
    {
        if (currentLogLevel >= LogLevel.Debug)
        {
            Debug.Log($"[DEBUG] {message}");
        }
    }
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogDebug(object message, Object context)
    {
        if (currentLogLevel >= LogLevel.Debug)
        {
            Debug.Log($"[DEBUG] {message}", context);
        }
    }
    public static void SetLogLevel(LogLevel level)
    {
        currentLogLevel = level;
    }
    public static void LoadLogLevel()
    {
        int savedLevel = PlayerPrefs.GetInt("LogLevel", (int)LogLevel.Info);
        currentLogLevel = (LogLevel)savedLevel;
    }
    public static void SaveLogLevel()
    {
        PlayerPrefs.SetInt("LogLevel", (int)currentLogLevel);
        PlayerPrefs.Save();
    }
}
