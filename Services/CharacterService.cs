using chrispserver.DbConfigurations;
using chrispserver.Handlers;
using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using SqlKata.Execution;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace chrispserver.Services;

public class CharacterService : ICharacter
{
    private readonly ConnectionManager _connectionManager;
    private readonly FeedHandler _feedHandler = new();
    private readonly PlayHandler _playHandler = new();

    private QueryFactory _gameDb => _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
    private QueryFactory _masterDb => _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);


    public CharacterService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    private async Task<UserCharacter?> GetActivatedCharacter(int userIndex)
    {
        var gameDb = _gameDb;
        return await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, userIndex)
                .Where(DbColumns.IsActive, true)
                .FirstOrDefaultAsync<UserCharacter>();
    }

    private async Task<Result> DeactivateOtherCharacters(int userIndex, int exceptCharacterIndex)
    {
        var gameDb = _gameDb;

        try
        {
            // 현재 활성화 되어있는 캐릭터 확인
            var otherActiveCharacters = await gameDb.Query(TableNames.UserCharacter)
                    .Where(DbColumns.UserIndex, userIndex)
                    .Where(DbColumns.IsActive, true)
                    .WhereNot(DbColumns.CharacterIndex, exceptCharacterIndex)
                    .GetAsync<UserCharacter>();

            // 장착 요청 캐릭터 외 다른 캐릭터 비활성화
            foreach (var character in otherActiveCharacters)
            {
                Console.WriteLine($"비활성화 {character.Character_Index}");

                await gameDb.Query(TableNames.UserCharacter)
                    .Where(DbColumns.UserIndex, userIndex)
                    .Where(DbColumns.CharacterIndex, character.Character_Index)
                    .UpdateAsync(new
                    {
                        Is_Active = false,
                        Level = character.is_acquired ? character.Level : 0,
                        Exp = character.is_acquired ? character.Exp : 0
                    });
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.ToString()} : 다른 캐릭터 비활성화 중 오류 발생");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }

    }

    public async Task<Result<Res_EquipCharacter>> EquipCharacterAsync(Req_EquipCharacter requestBody)
    {
        var gameDb = _gameDb;

        try
        {
            UserCharacter requestedCharacter = await gameDb.Query(TableNames.UserCharacter)
               .Where(DbColumns.UserIndex, requestBody.UserIndex)
               .Where(DbColumns.CharacterIndex, requestBody.CharacterIndex)
               .FirstOrDefaultAsync<UserCharacter>();

            // 요청한 캐릭터를 처음 장착할 경우
            if (requestedCharacter == null)
            {
                UserCharacter newCharacter = new UserCharacter
                {
                    User_Index = requestBody.UserIndex,
                    Character_Index = requestBody.CharacterIndex
                };

                await gameDb.Query(TableNames.UserCharacter).InsertAsync(newCharacter);

                requestedCharacter = newCharacter;
            }

            var deactiveResult = await DeactivateOtherCharacters(requestBody.UserIndex, requestBody.CharacterIndex);

            if (deactiveResult.ResultCodes != ResultCodes.Ok)
            {
                return Result<Res_EquipCharacter>.Fail(deactiveResult.ResultCodes);
            }


            var today = DateTime.Now;

            if (!requestedCharacter.Is_Active)
            {
                await gameDb.Query(TableNames.UserCharacter)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.CharacterIndex, requestBody.CharacterIndex)
                    .UpdateAsync(new
                    {
                        Is_Active = true,
                        Equipped_at = today
                    });
            }
            else
            {
                Console.WriteLine($"{requestedCharacter.Character_Index}는 이미 활성화된 캐릭터 ");
                return Result<Res_EquipCharacter>.Fail(ResultCodes.Equip_Fail_CharacterAlreadyEquipped);
            }

            return Result<Res_EquipCharacter>.Success(new Res_EquipCharacter
            {
                Level = requestedCharacter.Level,
                Exp = requestedCharacter.Exp,
                Equipped_at = today
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"아이템 장탈 오류 발생 {ex.ToString()}");
            return Result<Res_EquipCharacter>.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }
    public async Task<Result> EquipItemAsync(Req_EquipItem requestBody)
    {
        var gameDb = _gameDb;
        var masterDb = _masterDb;

        // 요청한 아이템 보유 확인
        UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
                .FirstOrDefaultAsync<UserEquip>();

        if (hasItem == null)
        {
            Console.WriteLine("요청한 아이템이 없습니다.");
            return Result.Fail(ResultCodes.Equip_Fail_NoItem);
        }

        // 현재 장착 캐릭터
        UserCharacter activatedCharacter = await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.IsActive, true)
                .FirstOrDefaultAsync<UserCharacter>();

        if (activatedCharacter == null)
        {
            Console.WriteLine("현재 활성화된 캐릭터가 없습니다.");
            return Result.Fail(ResultCodes.Equip_Fail_NoCharacter);
        }


        Item itemInfo = await masterDb.Query(TableNames.InfoItem)
                .Where(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
                .FirstOrDefaultAsync<Item>();

        if (itemInfo == null)
        {
            Console.WriteLine("아이템 정보가 없습니다");
            return Result.Fail(ResultCodes.Equip_Fail_NotExist);
        }

        if (itemInfo.Equip_Character_Index != activatedCharacter.Character_Index)
        {
            Console.WriteLine("현재 캐릭터에 장착할 수 없는 아이템 입니다.");
            return Result.Fail(ResultCodes.Equip_Fail_Incompatible);
        }

        // gameDb에서 캐릭터가 장착한 아이템들 확인
        var equippedItemIndexes = await gameDb.Query(TableNames.UserEquip)
            .Where(DbColumns.UserIndex, requestBody.UserIndex)
            .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
            .Where(DbColumns.IsEquipped, true)
            .WhereNot(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
            .Select(DbColumns.ItemIndex).GetAsync<int>();

        // masterDb에서 같은 item_type인 아이템 필터링
        var sameTypeItemIndexes = await masterDb.Query(TableNames.InfoItem)
            .Where(DbColumns.Type, itemInfo.Type)
            .WhereIn(DbColumns.ItemIndex, equippedItemIndexes)
            .Select(DbColumns.ItemIndex).GetAsync<int>();

        // gameDb에서 해당 item_index만 Update
        foreach (var equippedIndex in sameTypeItemIndexes)
        {
            await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, equippedIndex)
                .UpdateAsync(new { is_equipped = false });
        }

        UserEquip newEquipItem = await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
                .FirstOrDefaultAsync<UserEquip>();

        if (newEquipItem != null && newEquipItem.Is_Equipped)
        {
            Console.WriteLine("이미 장착한 아이템 입니다.");
            return Result.Fail(ResultCodes.Equip_Fail_ItemAlreadyEquipped);
        }

        if (newEquipItem == null)
        {
            await gameDb.Query(TableNames.UserEquip).InsertAsync(new
            {
                user_index = requestBody.UserIndex,
                character_index = activatedCharacter.Character_Index,
                item_index = requestBody.RequestedItemIndex,
                is_equipped = true
            });
        }
        else
        {
            await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
                .UpdateAsync(new { is_equipped = true });
        }

        return Result.Success();
    }


    public async Task<Result> UnequipItemAsnyc(Req_EquipItem requestBody)
    {
        var gameDb = _gameDb;

        try
        {
            var unequipItem = await gameDb.Query(TableNames.UserEquip)
                    .Where(DbColumns.UserIndex, requestBody.UserIndex)
                    .Where(DbColumns.ItemIndex, requestBody.RequestedItemIndex)
                    .UpdateAsync(new { is_equipped = false });

            return Result.Success();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"아이템 장탈 오류 발생 {ex.ToString()}");
            return Result.Fail(ResultCodes.Equip_Fail_Exception);
        }
    }



    public Task<Result<Res_Feed>> FeedAsync(Req_Feed requestBody)
        => UseGoodsAsync(requestBody.UserIndex, requestBody.GoodsIndex, requestBody.Quantity, _feedHandler);

    public Task<Result<Res_Play>> PlayAsync(Req_Play requestBody)
        => UseGoodsAsync(requestBody.UserIndex, requestBody.GoodsIndex, requestBody.Quantity, _playHandler);

    private async Task<Result<T>> UseGoodsAsync<T>(int userIndex, int goodsIndex, int quantity, IGoodsHandler<T> handler)
        where T : class, new()
    {
        // 요청한 goodsIndex가 먹이 타입인지 확인
        var masterDb = _masterDb;
        Goods goods = await masterDb.Query(TableNames.InfoGoods)
                .Where(DbColumns.GoodsIndex, goodsIndex)
                .FirstOrDefaultAsync<Goods>();

        if (goods == null || (Enums.GoodType)goods.Type != handler.RequiredGoodType)
        {
            Console.WriteLine("잘못된 재화 타입 입니다.");
            return Result<T>.Fail(ResultCodes.Goods_Fail_NotValidType);
        }

        // 요청한 재화의 수량 확인
        var gameDb = _gameDb;
        UserGoods userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, userIndex)
                .Where(DbColumns.GoodsIndex, goodsIndex)
                .FirstOrDefaultAsync<UserGoods>();

        if (userGoods == null)
        {
            Console.WriteLine("요청한 재화를 찾을 수 없습니다.");
            return Result<T>.Fail(ResultCodes.Goods_Fail_NotExist);
        }

        if (userGoods.Quantity < quantity)
        {
            Console.WriteLine("요청한 재화의 수량이 부족합니다.");
            return Result<T>.Fail(ResultCodes.Goods_Fail_NotEnough);
        }

        int remainQuantity = userGoods.Quantity - quantity;

        await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, userIndex)
                .Where(DbColumns.GoodsIndex, goodsIndex)
                .UpdateAsync(new { quantity = remainQuantity });

        return Result<T>.Success(handler.CreateResponse(remainQuantity));
    }
}
