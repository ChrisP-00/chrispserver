using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using MySqlConnector;
using SqlKata.Execution;
using System;
using System.Transactions;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Services;

public class MissionService : IMission
{
    private readonly ConnectionManager _connectionManager;
    private QueryFactory _gameDb => _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

    public MissionService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<Result> UpdateMissionProcessAsync(Req_PlayStatus requestBody, QueryFactory db, MySqlTransaction transaction)
    {
        var hasDailyMissions = await db.Query(TableNames.UserDailyMission)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .ExistsAsync(transaction);

        if (!hasDailyMissions)
        {
            Console.WriteLine("[Mission] 미션 진행도 업데이트 실패 : 유저가 일일 미션이 없음.");
            return Result.Fail(ResultCodes.Mission_Fail_NoMission);
        }

        var userDailyMissions = await db.Query(TableNames.UserDailyMission)
                        .Where(DbColumns.UserIndex, requestBody.UserIndex)
                        .Where(DbColumns.GoodsIndex, requestBody.GoodsIndex)
                        .WhereRaw($"{DbColumns.Mission_progress} < {DbColumns.Mission_Goal_Count}")
                        .IncrementAsync(DbColumns.Mission_progress, requestBody.Quantity);

        return Result.Success();
    }


    // 미션 수령은 따로 처리! 
    public async Task<Result> ReceiveMissionRewardAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission)
    {
        return await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
         {
             var userDailyMission = await db.Query(TableNames.UserDailyMission)
                       .Where(DbColumns.UserIndex, requestBody.UserIndex)
                       .Where(DbColumns.DailyMissionIndex, requestBody.DailyMissionIndex)
                       .WhereRaw($"{DbColumns.Mission_progress} = {DbColumns.Mission_Goal_Count}")
                       .UpdateAsync(new
                       {
                           is_received = true,
                           updated_at = DateTime.Now,
                       }, transaction: transaction);

             if (userDailyMission == 0)
             {
                 Console.WriteLine("[Mission] 미션 리워드 수령 실패 : 완료된 미션 없음");
                 return Result.Fail(ResultCodes.Mission_Fail_NoMission);
             }

             int rewardType = infoDailyMission.Reward_Type;
             int rewardAmount = infoDailyMission.Reward_Amount;

             var userGoods = await db.Query(TableNames.UserGoods)
                         .Where(DbColumns.UserIndex, requestBody.UserIndex)
                         .Where(DbColumns.GoodsIndex, rewardType)
                         .IncrementAsync(DbColumns.Quantity, rewardAmount, transaction: transaction);

             if (userGoods == 0)
             {
                 Console.WriteLine("[Mission] 미션 리워드 수령 실패 : 리워드 지급 안됌");
                 return Result.Fail(ResultCodes.Mission_Fail_NotAvailable);
             }

             return Result.Success();
         });
    }
}
