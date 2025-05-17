namespace chrispserver.ResReqModels;


public class Result
{
    public ResultCodes ResultCode { get; set; }
    public string? ResultMessage { get; set; }
    public bool IsSuccess => ResultCode == ResultCodes.Ok;


    public Result(ResultCodes resultCode, string? resultMessage)
    {
        ResultCode = resultCode;
        ResultMessage = resultMessage;
    }

    public static Result Success() => new Result(ResultCodes.Ok,ResultCodes.Ok.ToString());

    public static Result Fail(ResultCodes resultCode) => new Result(resultCode, resultCode.ToString());

    public static Result Fail(ResultCodes resultCode, string resultMessage) => new Result(resultCode, resultMessage);
}

public class Result<T>
{
    public ResultCodes ResultCode { get; set; }
    public string ResultMessage { get; set; }
    public T? Data { get; set; }
    public bool IsSuccess => ResultCode == ResultCodes.Ok;

    public Result(ResultCodes resultCodes, T? data = default)
    {
        ResultCode = resultCodes;
        ResultMessage = resultCodes.ToString();
        Data = data;
    }

    public static Result<T> Success(T data)
        => new Result<T>(ResultCodes.Ok, data);

    public static Result<T> Fail(ResultCodes resultCodes)
        => new Result<T>(resultCodes, default) { ResultMessage = resultCodes.ToString() };

    public static Result<T> Fail(ResultCodes resultCodes, string resultMessage)
        => new Result<T>(resultCodes, default) { ResultMessage = resultMessage };
}