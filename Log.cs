using System.Diagnostics;

namespace RingLib;

internal class Log
{
    public static Action<string> LoggerInfo;
    public static Action<string> LoggerError;
    public static void LogInfo(string key, string message)
    {
        if (LoggerInfo == null)
        {
            return;
        }
        LoggerInfo($"{key}: {message}");
    }
    public static void LogError(string key, string message)
    {
        if (LoggerError == null)
        {
            return;
        }
        StackTrace stackTrace = new StackTrace();
        LoggerError($"{key}: {stackTrace}\n{message}");
    }
}
