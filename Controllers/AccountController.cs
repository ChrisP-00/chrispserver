using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.ResReqModels.Response;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Controllers;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly IAccount _account;

    public AccountController(IAccount account)
    {
        _account = account;
    }

    /// <summary>
    /// 회원가입 요청
    /// </summary>
    [HttpPost("Create-Account")]
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
}
