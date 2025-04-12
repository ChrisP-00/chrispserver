using chrispserver.ResReqModels;
using MySqlConnector;
using SqlKata.Execution;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Services;

public interface IMission
{
    Task<Result> UpdateMissionProcessAsync(Req_PlayStatus requestBody, QueryFactory db, MySqlTransaction transaction);

    Task<Result> ReceiveMissionRewardAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission);
}
