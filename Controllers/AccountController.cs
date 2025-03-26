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
    public async Task<Result> CreateAccountAsync([FromBody] Req_CreateAccount request)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [CreateAccount Request] " + JsonSerializer.Serialize(request));
        return await _account.CreateAccountAsync(request);
    }

    /// <summary>
    /// 로그인 요청
    /// </summary>
    [HttpPost("Login")]
    public async Task<Result<Res_Login>> LoginAsync([FromBody] Req_Login request)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Login Request] " + JsonSerializer.Serialize(request));
        return await _account.LoginAsync(request);
    }



}
