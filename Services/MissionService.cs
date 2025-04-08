using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using SqlKata.Execution;
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

    public async Task<Result> UpdateMissionProcessAsync(int userIndex, int goodsIndex, int quantity)
    {
        try
        {
            using var gameDb = _gameDb;

            var hasDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                    .Where(DbColumns.UserIndex, userIndex)
                    .ExistsAsync();

            if (!hasDailyMissions)
            {
                Console.WriteLine("[Mission] 미션 진행도 업데이트 실패 : 유저가 일일 미션이 없음.");
                return Result.Fail(ResultCodes.Mission_Fail_Exception);
            }

            var userDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                            .Where(DbColumns.UserIndex, userIndex)
                            .Where(DbColumns.GoodsIndex, goodsIndex)
                            .WhereRaw($"{DbColumns.Mission_progress} < {DbColumns.Mission_Goal_Count}")
                            .IncrementAsync(DbColumns.Mission_progress, quantity);

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Mission] 미션 진행도 업데이트 실패 : {ex.ToString()}");
            return Result.Fail(ResultCodes.Mission_Fail_Exception);
        }
    }

    public async Task<Result> ReceiveMissionRewardAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission)
    {
        try
        {
            var result = await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
            {
                var userDailyMission = await db.Query(TableNames.UserDailyMission)
                          .Where(DbColumns.UserIndex, requestBody.UserIndex)
                          .Where(DbColumns.DailyMissionIndex, requestBody.DailyMissionIndex)
                          .WhereRaw($"{DbColumns.Mission_progress} = {DbColumns.Mission_Goal_Count}")
                          .UpdateAsync(new
                          {
                              is_received = true,
                              updated_at = DateTime.Now,
                          }, transaction : transaction);

                if (userDailyMission == 0)
                {
                    throw new Exception("[Mission] 미션 리워드 수령 실패 : 완료된 미션 없음");
                }

                int rewardType = infoDailyMission.Reward_Type;
                int rewardAmount = infoDailyMission.Reward_Amount;

                var userGoods = await db.Query(TableNames.UserGoods)
                            .Where(DbColumns.UserIndex, requestBody.UserIndex)
                            .Where(DbColumns.GoodsIndex, rewardType)
                            .IncrementAsync(DbColumns.Quantity, rewardAmount, transaction : transaction);

                if (userGoods == 0)
                {
                    throw new Exception("[Mission] 미션 리워드 수령 실패 : 리워드 지급 안됌");
                }
            });

            return result;
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"[Mission] 미션 리워드 수령 실패 : {ex.Message}");
            return Result.Fail(ResultCodes.Mission_Fail_Exception);
        }
    }
}
