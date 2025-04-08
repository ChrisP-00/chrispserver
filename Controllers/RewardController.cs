using chrispserver.ResReqModels;
using chrispserver.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Controllers;


[ApiController]
[Route("Reward")]
public class RewardController : ControllerBase
{
    private readonly IMission _mission;
    private readonly IMasterHandler _masterHandler;

    public RewardController(IMission mission, IMasterHandler masterHandler)
    {
        _mission = mission;
        _masterHandler = masterHandler;
    }


    [HttpPost("Receive-Mission")]
    public async Task<Result> ReceiveMissionAsync([FromBody] Req_ReceiveMission requestBody)
    {
        InfoDailyMission infoDailyMission = _masterHandler.GetInfoDataByIndex<InfoDailyMission>(requestBody.DailyMissionIndex);
        if (infoDailyMission == null)
        {
            Console.WriteLine($"Db에 존재하지 않는 아이템 인덱스 : {requestBody.DailyMissionIndex}");
            return Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }


        Console.WriteLine(">>>>>>>>>>>>>>>> [Receive-Mission Request] " + JsonSerializer.Serialize(requestBody));
        return await _mission.ReceiveMissionRewardAsync(requestBody, infoDailyMission);
    }
}
