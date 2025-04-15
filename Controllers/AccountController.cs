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
    /// 회원가입 요청
    /// </summary>
    [HttpPost("CreateAccount")]
    public async Task<Result> CreateAccountAsync([FromBody] Req_CreateAccount requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [CreateAccount Request] " + JsonSerializer.Serialize(requestBody));
        return await _account.CreateAccountAsync(requestBody);
    }

    /// <summary>
    /// 로그인 요청
    /// </summary>
    [HttpPost("Login")]
    public async Task<Result<Res_Login>> LoginAsync([FromBody] Req_Login requestBody)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Login Request] " + JsonSerializer.Serialize(requestBody));
        return await _account.LoginAsync(requestBody);
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
