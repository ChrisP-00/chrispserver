
using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using chrispserver.Securities;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using System;
using System.Collections.Specialized;
using System.Reflection.Metadata.Ecma335;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;
using static Humanizer.In;


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

    public async Task<Result<Res_Login>> LoginOrCreateAccountAsync(Req_Login requestBody)
    {
        if (!string.IsNullOrWhiteSpace(requestBody.MemberId))
        {
            // memberId로 계정 로그인
            var userAccountResult = await TryLoginWithMemberIdAsync(requestBody);
            Console.WriteLine($"result : {userAccountResult.ResultCode.ToString()}");
            if (userAccountResult.ResultCode == ResultCodes.Ok)
            {
                return userAccountResult;
            }
        }

        // device Id로 게스트 계정 로그인
        var guestAccountResult = await HandleGuestAccountAsync(requestBody);
        if (guestAccountResult.IsSuccess)
        {
            return guestAccountResult;
        }

        // 계정 생성 및 로그인
        return await CreateAndLoginNewAccountAsync(requestBody);
    }


    #region 내부 함수 정의
    private async Task<Result<Res_Login>> TryLoginWithMemberIdAsync(Req_Login requestBody)
    {
        try
        {
            var gameDb = _gameDb;

            var userAccount = await TryGetUserAccountByMemberIdAsync(requestBody.MemberId!, gameDb);

            if (userAccount.ResultCode != ResultCodes.Ok)
            {
                Console.WriteLine("Login Fail");
                return Result<Res_Login>.Fail(userAccount.ResultCode);
            }

            // Redis token 생성
            string token = await _redisAuthService.GenerateTokenAsync(requestBody.MemberId!, requestBody.DeviceId);

            // 마지막 로그인 시간 변경 
            await gameDb.Query(TableNames.UserAccount)
                .Where("user_index", userAccount.Data!.User_Index)
                .UpdateAsync(new
                {
                    Last_Login_At = DateTime.Now
                });

            var loginData = await BuildResLoginAsync(userAccount.Data, token);

            return Result<Res_Login>.Success(loginData.Data!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Account] 로그인 실패 : {ex.ToString()}");
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_Exception);
        }
    }

    private async Task<Result<User_Account>> TryLoginWithDeviceIdAsync(string deviceId)
    {
        var gameDb = _gameDb;

        var query = gameDb.Query(TableNames.UserAccount)
          .Where(DbColumns.Device_Id, deviceId)
          .WhereNull(DbColumns.MemberId);

        var dbUserAccount = await query.FirstOrDefaultAsync<UserAccount>();

        if (dbUserAccount == null)
        {
            Console.WriteLine($"[Account] 게스트 계정 없음 : device_Id = {deviceId}");
            return Result<User_Account>.Fail(ResultCodes.No_Guest_Account);
        }

        var userAccount = new User_Account
        {
            User_Index = dbUserAccount!.User_Index,
            Member_Id = dbUserAccount.Member_Id,
            Device_Id = dbUserAccount.Device_Id,
            Nickname = dbUserAccount.Nickname,
            Is_Banned = dbUserAccount.Is_Banned,
            Is_Deleted = dbUserAccount.Is_Deleted,
            Last_Login_At = dbUserAccount.Last_Login_At,
        };

        return Result<User_Account>.Success(userAccount);
    }

    private async Task<Result<User_Account>> TryGetUserAccountByMemberIdAsync(string memberId, QueryFactory db)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return Result<User_Account>.Fail(ResultCodes.No_MemberId);
        }

        var query = db.Query(TableNames.UserAccount)
                  .Where(DbColumns.MemberId, memberId);

        var dbUserAccount = await query.FirstOrDefaultAsync<UserAccount>();

        if (dbUserAccount == null)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정을 찾을 수 없음");
            return Result<User_Account>.Fail(ResultCodes.Login_Fail_NotUser);
        }

        // 해당 맴버 id 로 밴 당한지 확인 필요 
        if (dbUserAccount != null && dbUserAccount.Is_Banned)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정 정지 상태");
            return Result<User_Account>.Fail(ResultCodes.Ban_Account);
        }

        // 계정 삭제 유무 (계정이 있고 & 삭제됨) => 삭제된 계정
        if (dbUserAccount != null && dbUserAccount.Is_Deleted)
        {
            Console.WriteLine($"[Account] 로그인 실패 : 요청한 계정 삭제 상태");
            return Result<User_Account>.Fail(ResultCodes.Deleted_Account);
        }

        var userAccount = new User_Account
        {
            User_Index = dbUserAccount!.User_Index,
            Member_Id = dbUserAccount.Member_Id,
            Device_Id = dbUserAccount.Device_Id,
            Nickname = dbUserAccount.Nickname,
            Is_Banned = dbUserAccount.Is_Banned,
            Is_Deleted = dbUserAccount.Is_Deleted,
            Last_Login_At = dbUserAccount.Last_Login_At,
        };

        return Result<User_Account>.Success(userAccount);
    }

    private async Task<Result<Res_Login>> HandleGuestAccountAsync(Req_Login requestBody)
    {
        // device Id로 게스트 계정 로그인
        var guestAccount = await TryLoginWithDeviceIdAsync(requestBody.DeviceId!);
        Console.WriteLine($"게스트 계정 : {guestAccount.ResultCode.ToString()}");

        if (guestAccount.IsSuccess && guestAccount.Data != null)
        {
            var db = _gameDb;

            if (!string.IsNullOrWhiteSpace(requestBody.MemberId))
            {
                Console.WriteLine($"[LoginOrCreate] 기존 게스트 계정에 memberId 등록 : {requestBody.MemberId}");
                Result result = await UpdateGuestToMemberAccount(
                    guestAccount.Data.User_Index,
                    guestAccount.Data.Member_Id!,
                    guestAccount.Data.Nickname);

                if (result.ResultCode != ResultCodes.Ok)
                {
                    Console.WriteLine($"[UpdateGuestToMember]게스트 계정 MemberId 업데이트 오류 {guestAccount.Data.User_Index}");
                }
            }

            // Redis token 생성
            string token = await _redisAuthService.GenerateGuestTokenAsync(requestBody.DeviceId!);
            Console.WriteLine($"token: {token}");

            var loginData = await BuildResLoginAsync(guestAccount.Data, token);

            if (loginData.ResultCode != ResultCodes.Ok)
            {
                return Result<Res_Login>.Fail(loginData.ResultCode);
            }

            return loginData;
        }

        return Result<Res_Login>.Fail(ResultCodes.Account_GuestLogin_Fail);
    }

    private async Task<Result<Res_Login>> CreateAndLoginNewAccountAsync(Req_Login requestBody)
    {
        Req_CreateAccount req_CreateAccount = new Req_CreateAccount
        {
            MemberId = requestBody.MemberId,
            DeviceId = requestBody.DeviceId,
            Nickname = requestBody.Nickname,
        };

        var createAccountResult = await CreateAccountAsync(req_CreateAccount);
        if (createAccountResult.ResultCode != ResultCodes.Ok)
        {
            return Result<Res_Login>.Fail(createAccountResult.ResultCode);
        }

        var userAccount = await TryLoginWithDeviceIdAsync(requestBody.DeviceId!);

        if (!userAccount.IsSuccess && userAccount.Data == null)
        {
            Console.WriteLine($"fail : {userAccount.ResultCode.ToString()}");
            return Result<Res_Login>.Fail(userAccount.ResultCode);
        }

        // Redis token 생성
        string token = string.IsNullOrWhiteSpace(requestBody.MemberId)
            ? await _redisAuthService.GenerateGuestTokenAsync(requestBody.DeviceId!)
            : await _redisAuthService.GenerateTokenAsync(requestBody.MemberId!, requestBody.DeviceId!);

        Console.WriteLine($"token: {token}");

        return await BuildResLoginAsync(userAccount.Data!, token);
    }

    private async Task<Result> UpdateGuestToMemberAccount(int userIndex, string memberId, string? nickname)
    {
        Console.WriteLine($"[LoginOrCreate] 기존 게스트 계정에 memberId 등록 : {memberId}");

        var updateData = new Dictionary<string, object?>
        {
            ["Member_id"] = memberId
        };

        if (!string.IsNullOrWhiteSpace(nickname))
        {
            updateData["Nickname"] = nickname;
        }

        var db = _gameDb;

        int updatedRow = await db.Query(TableNames.UserAccount)
            .Where(DbColumns.UserIndex, userIndex)
            .UpdateAsync(updateData);

        if (updatedRow > 0)
        {
            return Result.Success();
        }

        return Result.Fail(ResultCodes.Create_Account_Fail);
    }

    private async Task<Result<Res_Login>> BuildResLoginAsync(User_Account userAccount, string token)
    {
        if (userAccount == null || string.IsNullOrWhiteSpace(token))
        {
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_Exception);
        }

        try
        {
            var gameDb = _gameDb;
            int userIndex = userAccount.User_Index;

            // 유저 캐릭터 정보
            var userCharacters = await gameDb.Query(TableNames.UserCharacter)
                 .Where(DbColumns.UserIndex, userIndex)
                 .GetAsync<UserCharacter>();

            var userInventories = await gameDb.Query(TableNames.UserInventory)
                .Where(DbColumns.UserIndex, userIndex)
                .GetAsync<UserInventory>();

            var userEquips = await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, userIndex)
                .GetAsync<UserEquip>();

            var userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, userIndex)
                .GetAsync<UserGoods>();

            var userDailyMissions = await gameDb.Query(TableNames.UserDailyMission)
                .Where(DbColumns.UserIndex, userIndex)
                .GetAsync<UserDailyMission>();

            foreach (var mission in userDailyMissions)
            {
                if (mission.Updated_At < DateTime.Today)
                {
                    if (mission.Mission_Progress > 0)
                    {
                        await gameDb.Query(TableNames.UserDailyMission)
                                .Where(DbColumns.UserIndex, userIndex)
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
                                .Where(DbColumns.UserIndex, userIndex)
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

            var resLogin = new Res_Login
            {
                Token = token,
                UserAccount = userAccount,
                UserCharacters = userCharacters.Select(c => new User_Character
                {
                    Character_Index = c.Character_Index,
                    Level = c.Level,
                    Exp = c.Exp,
                    Is_Active = c.Is_Active,
                    is_acquired = c.Is_acquired
                }).ToList(),
                UserInventories = userInventories.Select(c => new User_Inventory
                {
                    Item_Index = c.Item_Index
                }).ToList(),
                UserEquips = userEquips.Select(c => new User_Equip
                {
                    Character_Index = c.Character_Index,
                    Item_Type = c.Item_Type,
                    Item_Index = c.Item_Index
                }).ToList(),
                UserGoods = userGoods.Select(g => new User_Goods
                {
                    Goods_Index = g.Goods_Index,
                    Quantity = g.Quantity
                }).ToList(),
                UserDailyMission = userDailyMissions.Select(m => new User_Daily_Missions
                {
                    Daily_Mission_Index = m.Daily_Mission_Index,
                    Mission_Progress = m.Mission_Progress,
                    Is_Received = m.Is_Received
                }).ToList()
            };

            return Result<Res_Login>.Success(resLogin);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BuildResLogin] 예외 발생: {ex}");
            return Result<Res_Login>.Fail(ResultCodes.Login_Fail_Exception);
        }
    }

    private async Task<Result> CreateAccountAsync(Req_CreateAccount requestBody)
    {
        var transactionResult = await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
        {
            // define에서 가져오기
            string nickName = GetDefaultNickname(requestBody);

            Console.WriteLine($"닉네임 : {nickName}");

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

        if (transactionResult.ResultCode != ResultCodes.Ok)
        {
            return Result.Fail(transactionResult.ResultCode);
        }

        return Result.Success();
    }

    private string GetDefaultNickname(Req_CreateAccount requestBody)
    {
        var define = _masterHandler.GetInfoDataByIndex<InfoDefine>(NicknameDefineIndex);
        return string.IsNullOrWhiteSpace(requestBody.Nickname) ? define?.Description ?? "졸리" : requestBody.Nickname;
    }

    private async Task<int> CreateUserAccountAsync(Req_CreateAccount requestBody, string nickName, QueryFactory db, MySqlTransaction transaction)
    {
        string? memberId = string.IsNullOrWhiteSpace(requestBody.MemberId) ? null : requestBody.MemberId;

        return await db.Query(TableNames.UserAccount).InsertGetIdAsync<int>(new
        {
            Member_id = memberId,
            Device_id = requestBody.DeviceId,
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