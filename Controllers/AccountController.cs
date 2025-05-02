using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.ResReqModels.Response;
using static chrispserver.ResReqModels.Request;
using chrispserver.Securities;

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
        Console.WriteLine(">>>>>>>>>>>>>>>> [Login or Create Account Request] " + JsonSerializer.Serialize(requestBody));

        if (string.IsNullOrWhiteSpace(requestBody.UnityDeviceNumber))
        {
            Console.WriteLine($"[Controller] Unity Device Number 누락");
            return Result<Res_Login>.Fail(ResultCodes.InputData_MissingRequiredField);
        }

        return await _account.LoginOrCreateAccountAsync(requestBody);
    }

    /// <summary>
    /// 로그 아웃 만들기 
    /// </summary>
    [HttpPost("Logout")]
    public async Task<Result> LogoutAsync([FromHeader] string token)
    {
        token = token.Replace("Bearer ", "").Trim();
        bool success = await _redisAuthService.RevokeTokenAsync(token);
        return success ? Result.Success() : Result.Fail(ResultCodes.Logout_Fail_Exception);
    }
}
