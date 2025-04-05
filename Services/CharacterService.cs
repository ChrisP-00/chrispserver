using chrispserver.DbConfigurations;
using chrispserver.ResReqModels;
using SqlKata.Execution;
using System.Transactions;
using System;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using static chrispserver.ResReqModels.Request;
using static StackExchange.Redis.Role;
using System.Reflection;


namespace chrispserver.Services;

public class CharacterService : ICharacter
{
    private readonly ConnectionManager _connectionManager;

    private QueryFactory _gameDb => _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);

    public CharacterService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    #region 캐릭터, 아이템 장탈착 기능


    public async Task<Result> EquipCharacterAsync(Req_EquipCharacter requestBody)
    {
        var gameDb = _gameDb;

        try
        {
            UserCharacter currentCharacter = await gameDb.Query(TableNames.UserCharacter)
               .Where(DbColumns.UserIndex, requestBody.UserIndex)
               .Where(DbColumns.CharacterIndex, requestBody.EquipCharacterIndex)
               .FirstOrDefaultAsync<UserCharacter>();

            if (currentCharacter != null && currentCharacter.Is_Active)
            {
                Console.WriteLine($"{currentCharacter.Character_Index}는 이미 활성화된 캐릭터 ");
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

            int resetLevel = 5; // define에서 가져오기
            int defaultLevel = 0;
            int defaultEXP = 0;

            int AllActivatedCharacters = await gameDb.Query(TableNames.UserCharacter)
                       .Where(DbColumns.UserIndex, requestBody.UserIndex)
                       .Where(DbColumns.IsActive, true)
                       .CountAsync<int>();

            // 다른 활성화된 캐릭터가 있음
            if (AllActivatedCharacters > 1)
            {
                Console.WriteLine("현재 1개 초과의 캐릭터가 활성화 되어있습니다.");
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

            // 현재 장착한 캐릭터 아이템 정보 전달
            // UserEquip 테이블 전달 

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"아이템 장탈 오류 발생 {ex.ToString()}");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }

    public async Task<Result> EquipItemAsync(Req_EquipItem requestBody, InfoItem infoItem)
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
            Console.WriteLine("현재 활성화된 캐릭터가 없습니다.");

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

            Console.WriteLine($"{lastActivatedCharacter.Character_Index}으로 활성화 했습니다.");
        }



        if (infoItem.Equip_Character_Index != lastActivatedCharacter.Character_Index)
        {
            Console.WriteLine("현재 캐릭터에 장착할 수 없는 아이템 입니다.");
            return Result.Fail(ResultCodes.Equip_Fail_Incompatible);
        }

        // 요청한 아이템 보유 확인
        UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                .FirstOrDefaultAsync<UserEquip>();

        if (hasItem == null)
        {
            Console.WriteLine("요청한 아이템이 없습니다.");
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
            Console.WriteLine("이미 장착한 아이템 입니다.");
            return Result.Fail(ResultCodes.Equip_Fail_ItemAlreadyEquipped);
        }

        // 아이템 장탈
        await gameDb.Query(TableNames.UserEquip)
            .Where(DbColumns.CharacterIndex, lastActivatedCharacter.Character_Index)
            .Where(DbColumns.ItemType, infoItem.Item_Type)
            .WhereNot(DbColumns.ItemIndex, requestBody.EquipItemIndex)
            .DeleteAsync();

        Console.WriteLine("현재 활성화된 아이템 제거");

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
        var gameDb = _gameDb;

        try
        {
            // 현재 장착 캐릭터 확인
            UserCharacter lastActivatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.IsActive, true)
                    .FirstOrDefaultAsync<UserCharacter>();

            // 현재 장착 캐릭터가 없는 경우 처리
            if (lastActivatedCharacter == null)
            {
                // 로그 찍기
                Console.WriteLine("현재 활성화된 캐릭터가 없습니다.");

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

                Console.WriteLine($"{lastActivatedCharacter.Character_Index}으로 활성화 했습니다.");
            }

            // 요청한 아이템 보유 확인
            UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                    .FirstOrDefaultAsync<UserEquip>();

            if (hasItem == null)
            {
                Console.WriteLine("요청한 아이템이 없습니다.");
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
                Console.WriteLine("현재 장착하지 않은 아이템입니다.");
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
            Console.WriteLine($"아이템 장탈 오류 발생 {ex.ToString()}");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }
    #endregion

    #region 유저 재화 소비 관련 기능

    public async Task<Result> UseGoodsAsync(Req_UseGoods requestBody)
    {
        var gameDb = _gameDb;

        // 요청한 재화의 수량 확인
        UserGoods userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.GoodsIndex, requestBody.GoodsIndex)
                .FirstOrDefaultAsync<UserGoods>();

        if (userGoods == null)
        {
            Console.WriteLine("데이터를 찾을 수 없습니다.");
            return Result.Fail(ResultCodes.Goods_Fail_NotExist);
        }

        if (userGoods.Quantity < requestBody.Quantity)
        {
            Console.WriteLine("요청한 재화의 수량이 부족합니다.");
            return Result.Fail(ResultCodes.Goods_Fail_NotEnough);
        }

        int remainQuantity = userGoods.Quantity - requestBody.Quantity;

        await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.GoodsIndex, requestBody.GoodsIndex)
                .UpdateAsync(new { quantity = remainQuantity });

        return Result.Success();
    }
    #endregion
}
