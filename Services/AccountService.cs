
using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using chrispserver.Securities;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using System;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;


namespace chrispserver.Services;

public class AccountService : IAccount
{
    private readonly ConnectionManager _connectionManager;
    private readonly IMasterHandler _masterHandler;
    private readonly IRedisAuthService _redisAuthService;
    private QueryFactory _gameDb => _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

    private const int NicknameDefineIndex = 4;
    private const int DefaultFoodIndex = 1;
    private const int DefaultToyIndex = 2;
    private const int DefaultPointIndex = 3;

    public AccountService(ConnectionManager connectionManager, IMasterHandler masterHandler, IRedisAuthService redisAuthService)
    {
        _connectionManager = connectionManager;
        _masterHandler = masterHandler;
        _redisAuthService = redisAuthService;
    }

    public async Task<Result> CreateAccountAsync(Req_CreateAccount requestBody)
    {
        return await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
        {
            var (result, userAccount) = await TryGetUserAccountAsync(requestBody, db, transaction);

            if (!result.IsSuccess)
            {
                return result;
            }

            if (userAccount != null && !userAccount.Is_Deleted)
            {
                Console.WriteLine($"[Account] 계정 생성 실패 : 이미 가입한 계정");
                return Result.Fail(ResultCodes.Create_Account_Fail_Duplicate);
            }

            // define에서 가져오기
            string nickName = GetDefaultNickname(requestBody);

            // 1. UserAccount 생성
            int userIndex = await CreateUserAccountAsync(requestBody, nickName, db, transaction);
            if (userIndex <= 0)
            {
                Console.WriteLine("[CreateAccount] 계정 생성 실패 - 반환된 인덱스가 0 이하입니다.");
                return Result.Fail(ResultCodes.Create_Account_Fail_Exception);
            }
            Console.WriteLine($"user index : {userIndex} ");

            // 2. UserCharacter 생성
            int insertedCharacter = await CreateDefaultCharacterAsync(userIndex, db, transaction);
            if (insertedCharacter <= 0)
            {
                Console.WriteLine("[CreateAccount] UserCharacter 삽입 실패");
                return Result.Fail(ResultCodes.Create_Account_Fail_Exception);
            }

            // 3. UserGoods 생성
            await CreateInitialGoodsAsync(userIndex, db, transaction);

            // 4. 일일 미션 생성
            await CreateUserMissionsAsync(userIndex, db, transaction);

            return Result.Success();
        });
    }


    public async Task<Result<Res_Login>> LoginAsync(Req_Login requestBody)
    {
        try
        {
            var (result, userAccount) = await TryGetUserAccountAsync(requestBody, _gameDb);

            if (!result.IsSuccess || userAccount == null)
            {
                return Result<Res_Login>.Fail(result.ResultCode);
            }

            string token = await _redisAuthService.GenerateTokenAsync(requestBody.MemberId!);

            int userIndex = userAccount.User_Index;
            var gameDb = _gameDb;

            // 유저 캐릭터 정보
            var userCharacters = await gameDb.Query(TableNames.UserCharacter)
                 .Where("user_index", userIndex)
                 .GetAsync<UserCharacter>();

            // 유저 아이템 인벤토리 정보
            var userInventories = await gameDb.Query(TableNames.UserInventory)
                .Where("user_index", userIndex)
                .GetAsync<UserInventory>();

            // 유저 캐릭터 아이템 장착 정보
            var userEquips = await gameDb.Query(TableNames.UserEquip)
                .Where("user_index", userIndex)
                .GetAsync<UserEquip>();

            // 유저 재화 정보
            var userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where("user_index", userIndex)
                .GetAsync<UserGoods>();

            // 유저 일일 미션 정보
            var userDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                .Where("user_index", userIndex)
                .GetAsync<UserDailyMission>();

            foreach (var mission in userDailyMissions)
            {
                if (mission.Updated_At < DateTime.Today)
                {
                    if (mission.Mission_Progress > 0)
                    {
                        await gameDb.Query(TableNames.UserDailyMission)
                                .Where(DbColumns.UserIndex, userAccount.User_Index)
                                .Where(DbColumns.DailyMissionIndex, mission.Daily_Mission_Index)
                                .UpdateAsync(new
                                {
                                    mission_progress = 0,
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

            var _userInventory = userInventories.Select(c => new User_Inventory
            {
                Item_Index = c.Item_Index
            }).ToList();

            var _userEquip = userEquips.Select(c => new User_Equip
            {
                Character_Index = c.Character_Index,
                Item_Type = c.Item_Type,
                Item_Index = c.Item_Index
            });

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
                Token = token,
                UserAccount = userAccount,
                UserCharacters = _userCharacter.ToList(),
                UserInventories = _userInventory.ToList(),
                UserEquips = _userEquip.ToList(),
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
        catch (Exception ex)
        {
            Console.WriteLine($"[Account] 로그인 실패 : {ex.ToString()}");
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_Exception);
        }
    }


    #region 내부 함수 정의
    private async Task<(Result result, User_Account? Account)> TryGetUserAccountAsync<T>(T requestBody, QueryFactory db, MySqlTransaction? transaction = null) where T : IMemberId
    {
        var query = db.Query(TableNames.UserAccount)
                  .Where(DbColumns.MemberId, requestBody.MemberId);

        var dbUserAccount = transaction != null ? await query.FirstOrDefaultAsync<UserAccount>(transaction) : await query.FirstOrDefaultAsync<UserAccount>();

        if (dbUserAccount == null)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정을 찾을 수 없음");
            return (Result.Fail(ResultCodes.Login_Fail_NotUser), null);
        }

        // 해당 맴버 id 로 밴 당한지 확인 필요 
        if (dbUserAccount != null && dbUserAccount.Is_Banned)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정 정지 상태");
            return (Result.Fail(ResultCodes.Ban_Account), null);
        }

        // 계정 삭제 유무 (계정이 있고 & 삭제됨) => 삭제된 계정
        if (dbUserAccount != null && dbUserAccount.Is_Deleted)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정 삭제 상태");
            return (Result.Fail(ResultCodes.Deleted_Account), null);
        }

        var userAccount = new User_Account
        {
            User_Index = dbUserAccount.User_Index,
            Nickname = dbUserAccount.Nickname,
            Is_Banned = dbUserAccount.Is_Banned,
            Is_Deleted = dbUserAccount.Is_Deleted,  
            Last_Login_At  = dbUserAccount.Last_Login_At,
        };

        return (Result.Success(), userAccount);
    }

    private string GetDefaultNickname(Req_CreateAccount requestBody)
    {
        var define = _masterHandler.GetInfoDataByIndex<InfoDefine>(NicknameDefineIndex);
        return string.IsNullOrWhiteSpace(requestBody.Nickname) ? define?.Description ?? "졸리" : requestBody.Nickname;
    }

    private async Task<int> CreateUserAccountAsync(Req_CreateAccount requestBody, string nickName, QueryFactory db, MySqlTransaction transaction)
    {
        return await db.Query(TableNames.UserAccount).InsertGetIdAsync<int>(new
        {
            Member_id = requestBody.MemberId,
            Unity_device_number = requestBody.UnityDeviceNumber,
            Nickname = nickName
        }, transaction);
    }

    private async Task<int> CreateDefaultCharacterAsync(int userIndex, QueryFactory db, MySqlTransaction transaction)
    {
        return await db.Query(TableNames.UserCharacter).InsertAsync(new
        {
            user_index = userIndex,
            character_index = 1
        }, transaction);
    }

    private async Task CreateInitialGoodsAsync(int userIndex, QueryFactory db, MySqlTransaction transaction)
    {
        int food = _masterHandler.GetDefaultValueOrDefault(DefaultFoodIndex, 5, "Food");
        int toy = _masterHandler.GetDefaultValueOrDefault(DefaultToyIndex, 5, "Toy");
        int point = _masterHandler.GetDefaultValueOrDefault(DefaultPointIndex, 0, "Point");

        var goodsList = new[] {
            new { user_index = userIndex, goods_index = 1, quantity = food },
            new { user_index = userIndex, goods_index = 2, quantity = toy },
            new { user_index = userIndex, goods_index = 3, quantity = point }
        };

        foreach (var goods in goodsList)
        {
            int goodsInserted = await db.Query(TableNames.UserGoods).InsertAsync(goods, transaction: transaction);

            if (goodsInserted <= 0)
            {
                throw new Exception($"[CreateAccount] UserGoods 삽입 실패 - goods_index: {goods.goods_index}");
            }
        }
    }

    private async Task CreateUserMissionsAsync(int userIndex, QueryFactory db, MySqlTransaction transaction)
    {
        List<InfoDailyMission>? missions = _masterHandler.GetAll<InfoDailyMission>();
        if (missions == null || missions.Count == 0)
        {
            throw new Exception($"[CreateAccount] 마스터 데이터에 일일 미션 없음");
        }

        foreach (var mission in missions)
        {
            int missionInsert = await db.Query(TableNames.UserDailyMission).InsertAsync(new
            {
                user_index = userIndex,
                daily_mission_index = mission.Daily_Mission_Index,
                goods_type = mission.Goods_Type,
                goods_index = mission.Goods_Index,
                mission_goal_count = mission.Mission_Goal_Count,
            }, transaction: transaction);

            if (missionInsert <= 0)
            {
                throw new Exception($"[DailyMission] DailyMission 삽입 실패 - daily_mission_index: {mission.Daily_Mission_Index}");
            }
        }
    }
    #endregion
}