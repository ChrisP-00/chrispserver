using chrispserver.ResReqModels;
using System.Collections.Concurrent;
using System.Text;

namespace chrispserver.Utilities;

public static class LogManager
{
    private static ILoggerFactory? _loggerFactory;
    private static ILogger? _globalLogger;
    private static readonly BlockingCollection<string> _fileLogQueue = new(new ConcurrentQueue<string>());
    private static readonly CancellationTokenSource _cts = new();
    private static readonly string _logDirectory;

    static LogManager()
    {
        _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        if (!Directory.Exists(_logDirectory))
            Directory.CreateDirectory(_logDirectory);

        Task.Run(() => ProcessFileLogQueue(_cts.Token));
    }

    public static void SetLoggerFactory(ILoggerFactory loggerFactory, string categoryName)
    {
        _loggerFactory = loggerFactory;
        _globalLogger = _loggerFactory.CreateLogger(categoryName);
    }

    public static ILogger<T> GetLogger<T>() where T : class
    {
        if (_loggerFactory == null)
            throw new InvalidOperationException("LoggerFactory is not set.");
        return _loggerFactory.CreateLogger<T>();
    }

    // ---------- 콘솔 로그 전용 ----------
    public static void Info(string msg) => _globalLogger?.LogInformation(msg);
    public static void Warn(string msg) => _globalLogger?.LogWarning(msg);
    public static void Error(string msg) => _globalLogger?.LogError(msg);
    public static void Error(Exception ex) => _globalLogger?.LogError(ex, ex.Message);

    // ---------- 상황별 로그 메서드 (외부용) ----------
    public static void LogUserContentError(int userIndex, string contentName, ResultCodes resultCode)
        => LogFailure(userIndex, contentName, resultCode);

    public static void LogReceiveMissionError(int userIndex, Exception e, string input)
        => LogFailure(userIndex, "ReceiveMission", ResultCodes.Mission_Fail_Exception, input, e);

    public static void LogAuthFailure(string token, string? memberId, string deviceId, string reason)
    {
        string log = $"Auth Fail - Reason: {reason}, Token: {token}, MemberId: {memberId}, DeviceId: {deviceId}";
        _fileLogQueue.Add($"[{Now()}] [AuthFail] {log}");
    }

    public static void LogUserAccountUpdate(int userIndex, string action, string? before = null, string? after = null)
    {
        var log = new StringBuilder();
        log.Append($"[User:{userIndex}] Account Update - {action}");

        if (before != null || after != null)
            log.Append($"\nBefore: {before}\nAfter: {after}");

        _fileLogQueue.Add($"[{Now()}] [AccountUpdate] {log}");
    }

    // ---------- 내부 공통 처리 ----------
    private static void LogFailure(
        int userIndex,
        string contentName,
        ResultCodes resultCode,
        string? input = null,
        Exception? ex = null)
    {
        var sb = new StringBuilder();
        sb.Append($"[User:{userIndex}] [Content:{contentName}] Code: {resultCode}");

        if (!string.IsNullOrWhiteSpace(input))
            sb.Append($"\nInput: {input}");

        if (ex != null)
        {
            sb.Append($"\nError: {ex.Message}");
            sb.Append($"\nStackTrace: {ex.StackTrace}");
        }

        _fileLogQueue.Add($"[{Now()}] [Failure] {sb}");
    }

    private static string Now() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    private static async Task ProcessFileLogQueue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var msg = _fileLogQueue.Take(token);
                string file = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
                await File.AppendAllTextAsync(file, msg + Environment.NewLine);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _globalLogger?.LogError(ex, "[HybridLogManager] File logging failed");
            }
        }
    }

    public static void Dispose()
    {
        _cts.Cancel();
        _fileLogQueue.CompleteAdding();

        while (_fileLogQueue.TryTake(out var remain))
        {
            string file = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");
            File.AppendAllText(file, remain + Environment.NewLine);
        }
    }
}
