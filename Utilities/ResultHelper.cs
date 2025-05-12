using chrispserver.ResReqModels;

namespace chrispserver.Utilities;

public static class ResultHelper
{
    public static Result SuccessWithLog(int userIndex, LogCategory category, string action, string? before = null, string? after = null)
    {
        LogManager.LogUserAccountUpdate(userIndex, $"[{category}] {action}", before, after);
        return Result.Success();
    }

    public static Result<T> SuccessWithLog<T>(int userIndex, LogCategory category, string action, T data, string? before = null, string? after = null)
    {
        LogManager.LogUserAccountUpdate(userIndex, $"[{category}] {action}", before, after);
        return Result<T>.Success(data);
    }

    public static Result FailWithLog(int userIndex, LogCategory category, ResultCodes code, string? reason = null)
    {
        LogManager.LogUserContentError(userIndex, category.ToString(), code);
        return Result.Fail(code, reason ?? code.ToString());
    }

    public static Result FailWithException(int userIndex, LogCategory category, ResultCodes code, string? input = null, Exception? ex = null)
    {
        LogManager.LogReceiveMissionError(userIndex, ex ?? new Exception("No stacktrace"), input ?? "no input");
        return Result.Fail(code);
    }

    public static Result AuthFailWithLog(string token, string? memberId, string deviceId, string reason, ResultCodes code)
    {
        LogManager.LogAuthFailure(token, memberId, deviceId, reason);
        return Result.Fail(code, reason);
    }
}