using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using SqlKata;
using SqlKata.Execution;
using System.Transactions;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using static chrispserver.ResReqModels.Request;


namespace chrispserver.Services;

public class CharacterService : ICharacter
{
    private readonly ConnectionManager _connectionManager;
    private readonly IMission _mission;
    private readonly IMasterHandler _masterHandler;

    /// <summary>
    /// 트랜잭션 외부에서만 사용하는 QueryFactory
    /// ExecuteInTransactionAsync 내부에서 사용 금지
    /// </summary>
    private QueryFactory _gameDb => _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
    public CharacterService(ConnectionManager connectionManager, IMission mission, IMasterHandler masterHandler)
    {
        _connectionManager = connectionManager;
        _mission = mission;
        _masterHandler = masterHandler;
    }

    #region 캐릭터, 아이템 장탈착 기능
    public async Task<Result> EquipCharacterAsync(Req_EquipCharacter requestBody)
    {
        if (_masterHandler.IsValid<InfoCharacter>(requestBody.EquipCharacterIndex))
        {
            Console.WriteLine($"Db에 존재하지 않는 캐릭터 인덱스 : {requestBody.EquipCharacterIndex}");
            Result.Fail(ResultCodes.Equip_Fail_CharacterNotExist);
        }

        try
        {
            using var gameDb = _gameDb;

            UserCharacter currentCharacter = await gameDb.Query(TableNames.UserCharacter)
               .Where(DbColumns.UserIndex, requestBody.UserIndex)
               .Where(DbColumns.CharacterIndex, requestBody.EquipCharacterIndex)
               .FirstOrDefaultAsync<UserCharacter>();

            if (currentCharacter != null && currentCharacter.Is_Active)
            {
                Console.WriteLine($"[Character] 캐릭터 장착 실패 : {currentCharacter.Character_Index}는 이미 활성화된 캐릭터");
                return Result.Fail(ResultCodes.Equip_Fail_CharacterAlreadyEquipped);
            }

            // 요청한 캐릭터를 처음 장착할 경우
            if (currentCharacter == null)
            {
                UserCharacter newCharacter = new UserCharacter
                {
                    User_Index = requestBody.UserIndex,
                    Character_Index = requestBody.EquipCharacterIndex
                };

                await gameDb.Query(TableNames.UserCharacter).InsertAsync(newCharacter);

                currentCharacter = newCharacter;
            }

            int resetLevel = _masterHandler.GetDefaultValueOrDefault(5, 5, "Level Reset");
            int defaultLevel = 0;
            int defaultEXP = 0;

            int AllActivatedCharacters = await gameDb.Query(TableNames.UserCharacter)
                       .Where(DbColumns.UserIndex, requestBody.UserIndex)
                       .Where(DbColumns.IsActive, true)
                       .CountAsync<int>();

            // 다른 활성화된 캐릭터가 있음
            if (AllActivatedCharacters > 1)
            {
                Console.WriteLine("[Character] 캐릭터 장착 오류 : 현재 1개 초과의 캐릭터가 활성화 되어있습니다.");
            }

            UserCharacter currentActivatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                     .Where(DbColumns.UserIndex, requestBody.UserIndex)
                     .Where(DbColumns.IsActive, true)
                     .FirstOrDefaultAsync<UserCharacter>();

            // 기존에 활성화 되어있는 캐릭터 레벨 확인 및 초기화
            if (currentActivatedCharacter.Level >= resetLevel)
            {
                var otherActiveCharacters = await gameDb.Query(TableNames.UserCharacter)
                        .Where(DbColumns.UserIndex, requestBody.UserIndex)
                        .Where(DbColumns.IsActive, true)
                        .UpdateAsync(new
                        {
                            Is_Active = false
                        });
            }
            else
            {
                var otherActiveCharacters = await gameDb.Query(TableNames.UserCharacter)
                        .Where(DbColumns.UserIndex, requestBody.UserIndex)
                        .Where(DbColumns.IsActive, true)
                        .UpdateAsync(new
                        {
                            Is_Active = false,
                            Level = defaultLevel,
                            EXP = defaultEXP,
                        });
            }

            var today = DateTime.Now;
            await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, requestBody.EquipCharacterIndex)
                .UpdateAsync(new
                {
                    Is_Active = true,
                    Equipped_At = today
                });

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Character] 캐릭터 장착 실패 : {ex.ToString()}");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }

    public async Task<Result> EquipItemAsync(Req_EquipItem requestBody)
    {
        var gameDb = _gameDb;

        InfoItem? infoItem = _masterHandler.GetInfoDataByIndex<InfoItem>(requestBody.EquipItemIndex);

        if (infoItem == null)
        {
            Console.WriteLine($"Db에 존재하지 않는 아이템 인덱스 : {requestBody.EquipItemIndex}");
            return Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }

        // 현재 장착 캐릭터 확인
        UserCharacter? lastActivatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.IsActive, true)
                .FirstOrDefaultAsync<UserCharacter>();

        // 현재 장착 캐릭터가 없는 경우 처리
        if (lastActivatedCharacter == null)
        {
            lastActivatedCharacter = await EquipLastActivatedCharacterAsync(gameDb, requestBody);

            if (lastActivatedCharacter == null)
            {
                return Result.Fail(ResultCodes.Equip_Fail_CharacterNotExist);
            }
        }

        if (infoItem.Equip_Character_Index != lastActivatedCharacter.Character_Index)
        {
            Console.WriteLine("[Item] 아이템 장착 실패 : 현재 캐릭터에 장착할 수 없는 아이템");
            return Result.Fail(ResultCodes.Equip_Fail_Incompatible);
        }

        // 요청한 아이템 보유 확인
        UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                .FirstOrDefaultAsync<UserEquip>();

        if (hasItem == null)
        {
            Console.WriteLine("[Item] 아이템 장착 실패 : 요청한 아이템이 없음");
            return Result.Fail(ResultCodes.Equip_Fail_NoItem);
        }

        // 현재 장착한 아이템 정보 확인 
        UserEquip newEquipItem = await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                .FirstOrDefaultAsync<UserEquip>();

        if (newEquipItem != null)
        {
            Console.WriteLine("[Item] 아이템 장착 실패 : 이미 장착한 아이템");
            return Result.Fail(ResultCodes.Equip_Fail_ItemAlreadyEquipped);
        }

        // 아이템 장탈
        await gameDb.Query(TableNames.UserEquip)
            .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index)
            .Where(DbColumns.ItemType, infoItem.Item_Type)
            .WhereNot(DbColumns.ItemIndex, requestBody.EquipItemIndex)
            .DeleteAsync();

        Console.WriteLine("[Item] 아이템 장착 : 현재 활성화된 아이템 제거");

        // 로그에 추가

        await gameDb.Query(TableNames.UserEquip).InsertAsync(new
        {
            user_index = requestBody.UserIndex,
            character_index = lastActivatedCharacter.Character_Index,
            item_type = infoItem.Item_Type,
            item_index = requestBody.EquipItemIndex,
        });

        return Result.Success();
    }

    public async Task<Result> UnequipItemAsnyc(Req_EquipItem requestBody)
    {
        try
        {
            var gameDb = _gameDb;

            // 현재 장착 캐릭터 확인
            UserCharacter lastActivatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.IsActive, true)
                    .FirstOrDefaultAsync<UserCharacter>();

            // 현재 장착 캐릭터가 없는 경우 처리
            if (lastActivatedCharacter == null)
            {
                // 로그 찍기
                Console.WriteLine("[Item] 아이템 장탈 오류 : 현재 활성화된 캐릭터가 없음");

                // 마지막에 장착한 캐릭터
                lastActivatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                        .Where(DbColumns.UserIndex, requestBody.UserIndex)
                        .OrderByDesc(DbColumns.EquippedAt)
                        .FirstOrDefaultAsync<UserCharacter>();

                await gameDb.Query(TableNames.UserCharacter)
                        .Where(DbColumns.UserIndex, requestBody.UserIndex)
                        .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index)
                        .UpdateAsync(new
                        {
                            is_Equipped = true,
                            equipped_At = DateTime.Now
                        });

                Console.WriteLine($"[Item] 아이템 장탈 오류 : {lastActivatedCharacter.Character_Index}으로 활성화");
            }

            // 요청한 아이템 보유 확인
            UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                    .FirstOrDefaultAsync<UserEquip>();

            if (hasItem == null)
            {
                Console.WriteLine("[Item] 아이템 장탈 실패 : 요청한 아이템 없음");
                return Result.Fail(ResultCodes.Equip_Fail_NoItem);
            }

            // 요청한 아이템을 장착 중인지
            int countEquipItem = await gameDb.Query(TableNames.UserEquip)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index)
                    .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                    .CountAsync<int>();

            if (countEquipItem == 0)
            {
                Console.WriteLine("[Item] 아이템 장탈 실패 : 현재 장착하지 않은 아이템");
                return Result.Fail(ResultCodes.Equip_Fail_NoEquippedItem);
            }

            await gameDb.Query(TableNames.UserEquip)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                    .DeleteAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Item] 아이템 장탈 실패 : {ex.ToString()}");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }
    #endregion

    #region 캐릭터 상태 업데이트 관련 기능

    public async Task<Result> PlayStatusAsync(Req_PlayStatus requestBody)
    {
        if (_masterHandler.IsValid<InfoItem>(requestBody.GoodsIndex))
        {
            Console.WriteLine($"Db에 존재하지 않는 아이템 인덱스 : {requestBody.GoodsIndex}");
            Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }

        return await _connectionManager.ExecuteInTransactionAsync(DbKeys.GameServerDB, async (db, transaction) =>
        {
            // 1. 재화 소모
            var useGoodsResult = await UseGoodsAsync(requestBody, db, transaction);
            if (useGoodsResult.ResultCode != ResultCodes.Ok)
            {
                return useGoodsResult;
            }

            // 2. 미션 업데이트
            var missionResult = await _mission.UpdateMissionProcessAsync(requestBody, db, transaction);
            if (missionResult.ResultCode != ResultCodes.Ok)
            {
                return missionResult;
            }

            return Result.Success();
        });
    }

    public async Task<Result> UseGoodsAsync(Req_PlayStatus requestBody, QueryFactory db, MySqlTransaction transaction)
    {
        // 요청한 재화의 수량 확인
        UserGoods userGoods = await db.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.GoodsIndex, requestBody.GoodsIndex)
                .FirstOrDefaultAsync<UserGoods>(transaction);

        if (userGoods == null)
        {
            Console.WriteLine("[Goods] 재화 사용 실패 : 데이터를 찾을 수 없음");
            return Result.Fail(ResultCodes.Goods_Fail_NotExist);
        }

        if (userGoods.Quantity < requestBody.Quantity)
        {
            Console.WriteLine("[Goods] 재화 사용 실패 : 요청한 재화의 수량 부족");
            return Result.Fail(ResultCodes.Goods_Fail_NotEnough);
        }

        int remainQuantity = userGoods.Quantity - requestBody.Quantity;

        await db.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.GoodsIndex, requestBody.GoodsIndex)
                .UpdateAsync(new { quantity = remainQuantity }, transaction);

        return Result.Success();
    }

    public async Task<Result> UpdateCharacterExpAsync(Req_Exp requestBody, QueryFactory db, MySqlTransaction transaction)
    {
        UserCharacter? character = await db.Query(TableNames.UserCharacter)
          .Where(DbColumns.UserIndex, requestBody.UserIndex)
          .Where(DbColumns.IsActive, true)
          .FirstOrDefaultAsync<UserCharacter>(transaction);

        if (character == null)
        {
            character = await EquipLastActivatedCharacterAsync(db, requestBody, transaction);

            if (character == null)
            {
                Console.WriteLine($"[EXP] 활성화된 캐릭터가 없음 : 유저 {requestBody.UserIndex}");

                return Result.Fail(ResultCodes.EXP_Fail_CharacterNotExist);
            }
        }

        int currentLevel = character.Level;
        int totalExp = character.Exp + requestBody.Exp;

        while (true)
        {
            InfoLevel? levelInfo = _masterHandler.GetLevelInfo(character.Character_Index, currentLevel + 1);

            if (levelInfo != null)
            {
                Console.WriteLine($"level info: {levelInfo.Required_Exp}");
            }

            if (levelInfo == null || totalExp < levelInfo.Required_Exp)
            {
                break;
            }

            totalExp -= levelInfo.Required_Exp;
            currentLevel++;
        }

        await db.Query(TableNames.UserCharacter)
            .Where(DbColumns.UserIndex, requestBody.UserIndex)
            .Where(DbColumns.CharacterIndex, character.Character_Index)
            .UpdateAsync(new
            {
                Level = currentLevel,
                Exp = totalExp
            }, transaction);

        return Result.Success();
    }

    #endregion

    #region 내부 함수

    private async Task<UserCharacter?> EquipLastActivatedCharacterAsync(QueryFactory db, IUserAuth requestBody, MySqlTransaction? transaction = null)
    {
        Console.WriteLine("[Character] 캐릭터 비활성화 오류 : 현재 활성화된 캐릭터가 없음");

        // 마지막에 장착한 캐릭터
        UserCharacter lastActivatedCharacter = await db.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .OrderByDesc(DbColumns.EquippedAt)
                .FirstOrDefaultAsync<UserCharacter>(transaction);

        if (lastActivatedCharacter == null)
        {
            Console.WriteLine("[Character] 캐릭터 비활성화 오류 : 최근 장착된 캐릭터도 없음");
            return null;
        }

        var updateQuery = db.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index);

        if (transaction != null)
        {
            await updateQuery.UpdateAsync(new
            {
                is_active = true,
                equipped_At = DateTime.Now
            }, transaction);
        }
        else
        {
            await updateQuery.UpdateAsync(new
            {
                is_active = true,
                equipped_At = DateTime.Now
            });
        }

        Console.WriteLine($"[Character] 캐릭터 비활성화 오류 : {lastActivatedCharacter.Character_Index}으로 활성화");

        return lastActivatedCharacter;
    }
    #endregion
}
