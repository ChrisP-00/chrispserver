using chrispserver.ResReqModels;
using SqlKata.Execution;
using Microsoft.Data.SqlClient;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;
using System.Data.Common;

namespace chrispserver.Services;

public interface IMission
{
    Task<Result> UpdateMissionProcessAsync(Req_PlayStatus requestBody, QueryFactory db, DbTransaction transaction);

    Task<Result> ReceiveMissionRewardAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission);
}
