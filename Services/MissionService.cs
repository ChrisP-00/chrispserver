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
        var gameDb = _gameDb;
     
        var hasDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                .Where(DbColumns.UserIndex, userIndex)
                .ExistsAsync();

        if (!hasDailyMissions)
        {
            Console.WriteLine("유저가 일일 미션이 없음.");
            return Result.Fail(ResultCodes.Mission_Fail_Exception);
        }

        var userDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                        .Where(DbColumns.UserIndex, userIndex)
                        .Where(DbColumns.GoodsIndex, goodsIndex)
                        .WhereRaw($"{DbColumns.Mission_progress} < {DbColumns.Mission_Goal_Count}")
                        .IncrementAsync(DbColumns.Mission_progress, quantity);

        return Result.Success();
    }

    public async Task<Result> ReceiveMissionAsync(Req_ReceiveMission requestBody, InfoDailyMission infoDailyMission)
    {
        var gameDb = _gameDb;

        var userDailyMission = await gameDb.Query(TableNames.UserDailyMission)
                  .Where(DbColumns.UserIndex, requestBody.UserIndex)
                  .Where(DbColumns.DailyMissionIndex, requestBody.DailyMissionIndex)
                  .WhereRaw($"{DbColumns.Mission_progress} = {DbColumns.Mission_Goal_Count}")
                  .UpdateAsync(new
                  {
                      is_received = true,
                      updated_at = DateTime.Now,
                  });

        if(userDailyMission == 0)
        {
            Console.WriteLine("미션 수행 할게 없어...");
            Result.Fail(ResultCodes.Mission_Fail_Exception);
        }

        int rewardType = infoDailyMission.Reward_Type;
        int rewardAmount = infoDailyMission.Reward_Amount;

        var userGoods = await gameDb.Query(TableNames.UserGoods)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.GoodsIndex, rewardType)
                    .IncrementAsync(DbColumns.Quantity, rewardAmount);

        if (userGoods == 0) 
        {
            Console.WriteLine("리워드 지급 안됌");
            return Result.Fail(ResultCodes.Mission_Fail_Exception);
        }

        return Result.Success();
    }
}
