using chrispserver.ResReqModels;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Services;

public interface IMission
{
    Task<Result> UpdateMissionProcessAsync(int userIndex, int goodsIndex, int quantity);

    Task<Result> ReceiveMissionRewardAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission);
}
