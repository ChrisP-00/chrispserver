using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.ResReqModels.Response;
using static chrispserver.ResReqModels.Request;
using chrispserver.Securities;
using chrispserver.Utilities;
using static chrispserver.DbEntity.InfoEntities;

namespace chrispserver.Controllers;
[ApiController]
[Route("Test")]
public class TestController : ControllerBase
{
    private readonly IMasterHandler _masterHandler;

    public TestController(IMasterHandler masterHandler)
    {
        _masterHandler = masterHandler;
    }

    [HttpGet("CheckMissionCount")]
    public IActionResult CheckMissions()
    {
        var missions = _masterHandler.GetAll<InfoDailyMission>();
        return Ok(new
        {
            count = missions?.Count ?? 0
        });
    }

    [HttpGet("Characters")]
    public IActionResult GetCharacters()
    {
        var characters = _masterHandler.GetAll<InfoCharacter>() ?? new();
        return Ok(new { data = characters });
    }
}
