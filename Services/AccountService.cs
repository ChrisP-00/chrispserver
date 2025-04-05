
using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using SqlKata.Execution;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;


namespace chrispserver.Services;

public class AccountService : IAccount
{
    private readonly ConnectionManager _connectionManager;
    private readonly IMasterHandler _masterHandler;

    public AccountService(ConnectionManager connectionManager, IMasterHandler masterHandler)
    {
        _connectionManager = connectionManager;
        _masterHandler = masterHandler;
    }

    public async Task<Result> CreateAccountAsync(Req_CreateAccount requestBody)
    {
        using var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        UserAccount userAccount = await gameDb.Query(TableNames.UserAccount)
        .Where(DbColumns.MemberId, requestBody.MemberId)
        .FirstOrDefaultAsync<UserAccount>();

        if (userAccount != null)
        {
            return Result.Fail(ResultCodes.Create_Account_Fail_Duplicate);
        }

        // 해당 맴버 id 로 밴 당한지 확인 필요 
        if (userAccount != null && userAccount.Is_Banned)
        {
            return Result.Fail(ResultCodes.Ban_Account);
        }

        // 계정 삭제 유무 (계정이 있고 & 삭제가 되지 않았음) => 중복된 계정
        if (userAccount != null && !userAccount.Is_Deleted)
        {
            return Result.Fail(ResultCodes.Create_Account_Fail_Duplicate);
        }


        // define에서 가져오기
        try
        {
            InfoDefine? infoDefine = _masterHandler.GetInfoDataByIndex<InfoDefine>(4);
            string? rawNickname = infoDefine?.Description;  

            string defaultNickname = string.IsNullOrWhiteSpace(rawNickname) ? "졸리" : rawNickname;


            var result = await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
            {
                // 1. UserAccount 생성
                int index = await db.Query(TableNames.UserAccount).InsertGetIdAsync<int>(new
                {
                    Member_id = requestBody.MemberId,
                    Unity_device_number = requestBody.UnityDeviceNumber,
                    Nickname = string.IsNullOrWhiteSpace(requestBody.Nickname)
                                    ? defaultNickname
                                    : requestBody.Nickname
                }, transaction: transaction);

                Console.WriteLine($"user index : {index} ");

                if (index <= 0)
                {
                    throw new Exception("[CreateAccount] UserAccount 생성 실패: 반환된 index가 0 이하입니다.");
                }

                // 2. UserCharacter 생성
                int inserted = await db.Query(TableNames.UserCharacter).InsertAsync(new
                {
                    user_index = index,
                    character_index = 1     // 마스터 디비에서 가져오기 혹은 define에서 가져오기
                }, transaction: transaction);

                if (inserted <= 0)
                {
                    throw new Exception("[CreateAccount] UserCharacter 삽입 실패");
                }


                // define에서 가져오기
                // 3. UserGoods 여러 개 생성
                var userGoodsList = new[]
                {
                    new { user_index = index, goods_index = 1, quantity = 5 },
                    new { user_index = index, goods_index = 2, quantity = 5 },
                    new { user_index = index, goods_index = 3, quantity = 0 }
                };

                foreach (var goods in userGoodsList)
                {
                    int goodsInsert = await db.Query(TableNames.UserGoods).InsertAsync(goods, transaction: transaction);

                    if (goodsInsert <= 0)
                    {
                        throw new Exception($"[CreateAccount] UserGoods 삽입 실패 - goods_index: {goods.goods_index}");
                    }
                }


                // 4. 일일 미션 넣어주기
                foreach (var mission in _masterHandler.GetAll<InfoDailyMission>())
                {
                    int missionInsert = await db.Query(TableNames.UserDailyMission).InsertAsync(new
                    {
                        user_index = index,
                        mission_index = mission.Daily_Mission_Index,
                        goods_type = mission.Goods_Type,
                        goods_index = mission.Goods_Index,
                        complete_count = mission.Mission_Gaol_Count,
                    }, transaction: transaction);

                    if (missionInsert <= 0)
                    {
                        throw new Exception($"[DailyMission] DailyMission 삽입 실패 - daily_mission_index: {mission.Daily_Mission_Index}");
                    }
                }
            });

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return Result.Fail(ResultCodes.Create_Account_Fail_Exception);
        }
    }

    public async Task<Result<Res_Login>> LoginAsync(Req_Login requestBody)
    {

        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

        User_Account userAccount = await gameDb.Query(TableNames.UserAccount)
        .Where(DbColumns.MemberId, requestBody.MemberId)
        .FirstOrDefaultAsync<User_Account>();

        if (userAccount == null)
        {
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_NotUser);
        }

        // 해당 맴버 id 로 밴 당한지 확인 필요 
        if (userAccount != null && userAccount.Is_Banned)
        {
            return Result<Res_Login>.Fail(ResultCodes.Ban_Account);
        }

        // 계정 삭제 유무 (계정이 있고 & 삭제가 되지 않았음) => 중복된 계정
        if (userAccount != null && userAccount.Is_Deleted)
        {
            return Result<Res_Login>.Fail(ResultCodes.Deleted_Account);
        }

        int userIndex = userAccount.User_Index;

        var userCharacters = await gameDb.Query(TableNames.UserCharacter)
             .Where("user_index", userIndex)
             .GetAsync<UserCharacter>();

        var userInventories = await gameDb.Query(TableNames.UserInventory)
            .Where("user_index", userIndex)
            .GetAsync<UserInventory>();

        var userGoods = await gameDb.Query(TableNames.UserGoods)
            .Where("user_index", userIndex)
            .GetAsync<UserGoods>();

        var userDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
            .Where("user_index", userIndex)
            .GetAsync<UserDailyMission>();

  
        foreach(var mission in userDailyMissions)
        {
            if(mission.Updated_At < DateTime.Today)
            {
                if (mission.Mission_Progress > 0)
                {
                    await gameDb.Query(TableNames.UserDailyMission)
                            .Where(DbColumns.UserIndex, userAccount.User_Index)
                            .Where(DbColumns.DailyMissionIndex, mission.Daily_Mission_Index)
                            .UpdateAsync(new
                            {
                                process_count = 0,
                                is_received = false,
                                updated_at = DateTime.Now
                            });
                }
                else
                {
                    await gameDb.Query(TableNames.UserDailyMission)
                            .Where(DbColumns.UserIndex, userAccount.User_Index)
                            .Where(DbColumns.DailyMissionIndex, mission.Daily_Mission_Index)
                            .UpdateAsync(new
                            {
                                is_received = false,
                                updated_at = DateTime.Now
                            });
                }

                mission.Mission_Progress = 0;
                mission.Is_Received = false;
            }
        }

        var _userCharacter = userCharacters.Select(c => new User_Character
        {
            Character_Index = c.Character_Index,
            Level = c.Level,
            Exp = c.Exp,
            Is_Active = c.Is_Active,
            is_acquired = c.Is_acquired
        }).ToList();

        var _userInvetory = userInventories.Select(c => new User_Inventory
        {
             Item_Index = c.Item_Index
        }).ToList();

        var _userGoods = userGoods.Select(c => new User_Goods
        {
            Goods_Index = c.Goods_Index,
            Quantity = c.Quantity,
        }).ToList();

        var _userDailyMissions = userDailyMissions.Select(c => new User_Daily_Missions
        {
            Daily_Mission_Index = c.Daily_Mission_Index,
            Mission_Progress = c.Mission_Progress,
            Is_Received = c.Is_Received,
        }).ToList();


        var resultData = new Res_Login
        {
            UserAccount = userAccount,
            UserCharacters = _userCharacter.ToList(),
            UserInventories = _userInvetory.ToList(),
            UserGoods = _userGoods.ToList(),
            UserDailyMission = _userDailyMissions.ToList()
        };

       
        // 마지막 로그인 시간 변경 
        await gameDb.Query(TableNames.UserAccount)
            .Where("user_index", userIndex)
            .UpdateAsync(new
            {
                Last_Login_At = DateTime.Now
            });

        return Result<Res_Login>.Success(resultData);
    }


}