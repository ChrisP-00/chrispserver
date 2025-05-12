namespace chrispserver.Utilities;

public static class LogHelper
{
    public static void Info(LogCategory category, string message)
        => LogManager.Info($"[{category}] {message}");

    public static void Warn(LogCategory category, string message)
        => LogManager.Warn($"[{category}] {message}");

    public static void Error(LogCategory category, string message)
        => LogManager.Error($"[{category}] {message}");

    public static void Error(LogCategory category, Exception ex)
        => LogManager.Error(new Exception($"[{category}] {ex.Message}", ex));
}