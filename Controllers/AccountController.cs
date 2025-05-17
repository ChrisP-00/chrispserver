using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.ResReqModels.Response;
using static chrispserver.ResReqModels.Request;
using chrispserver.Securities;
using chrispserver.Utilities;

namespace chrispserver.Controllers;

[ApiController]
[Route("Account")]
public class AccountController : ControllerBase
{
    private readonly IAccount _account;
    private readonly IRedisAuthService _redisAuthService;

    public AccountController(IAccount account, IRedisAuthService redisAuthService)
    {
        _account = account;
        _redisAuthService = redisAuthService;
    }

    /// <summary>
    /// 로그인 요청
    /// </summary>
    [HttpPost("LoginOrCreateAccount")]
    public async Task<Result<Res_Login>> LoginOrCreateAccountAsync([FromBody] Req_Login requestBody)
    {
        LogManager.Info("[Account] LoginOrCreateAccout 요청 : " +  JsonSerializer.Serialize(requestBody));

        if (string.IsNullOrWhiteSpace(requestBody.DeviceId))
        {
            LogManager.Warn("[Account] Device Id 누락");
            LogManager.LogUserContentError(-1, "LoginOrCreateAccount", ResultCodes.InputData_MissingRequiredField);
            return Result<Res_Login>.Fail(ResultCodes.InputData_MissingRequiredField);
        }

        try
        {
            return await _account.LoginOrCreateAccountAsync(requestBody);
        }
        catch (Exception ex)
        {
            LogManager.Error(ex);
            LogManager.LogReceiveMissionError(-1, ex, JsonSerializer.Serialize(requestBody));
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_Exception);
        }
    }


    /// <summary>
    /// 로그 아웃
    /// </summary>
    [HttpPost("Logout")]
    public async Task<Result> LogoutAsync([FromHeader] string token)
    {
        token = token.Replace("Bearer ", "").Trim();
        bool success = await _redisAuthService.RevokeTokenAsync(token);
        return success ? Result.Success() : Result.Fail(ResultCodes.Logout_Fail_Exception);
    }
}
