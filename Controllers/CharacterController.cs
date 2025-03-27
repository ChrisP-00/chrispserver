using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using System.Text.Json;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Controllers;

[ApiController]
[Route("character")]
public class CharacterController : ControllerBase
{
    private readonly ICharacter _character;

    public CharacterController(ICharacter character)
    {
        _character = character;
    }

    /// <summary>
    /// 밥주기 요청
    /// </summary>
    [HttpPost("Feed")]
    public async Task<Result<Res_Feed>> FeedAsync([FromBody] Req_Feed request)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Feed Request] " + JsonSerializer.Serialize(request));
        return await _character.FeedAsync(request);
    }

    /// <summary>
    /// 놀아주기 요청
    /// </summary>
    [HttpPost("Play")]
    public async Task<Result<Res_Play>> PlayAsync([FromBody] Req_Play request)
    {
        Console.WriteLine(">>>>>>>>>>>>>>>> [Feed Request] " + JsonSerializer.Serialize(request));
        return await _character.PlayAsync(request);
    }
}
