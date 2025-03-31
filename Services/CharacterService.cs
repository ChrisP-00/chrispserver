using chrispserver.DbConfigurations;
using chrispserver.Handlers;
using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;
using static chrispserver.DbEntity.InfoEntities;
using static chrispserver.DbEntity.UserEntities;
using SqlKata.Execution;

namespace chrispserver.Services;

public class CharacterService : ICharacter
{
    private readonly ConnectionManager _connectionManager;
    private readonly FeedHandler _feedHandler = new();
    private readonly PlayHandler _playHandler = new();


    public CharacterService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<Result<Res_EquipCharacter>> EquipCharacterAsync(Req_EquipCharacter requestBody)
    {
        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
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

        // 현재 활성화 되어있는 캐릭터 확인
        var otherActiveCharacters = await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.IsActive, true)
                .WhereNot(DbColumns.CharacterIndex, requestBody.CharacterIndex)
                .GetAsync<UserCharacter>();

        // 중복 활성화된 캐릭터 비활성화
        foreach (var character in otherActiveCharacters)
        {
            character.Is_Active = false;

            if (!character.is_acquired)
            {
                character.Level = 0;
                character.Exp = 0;
            }

            Console.WriteLine($"비활성화 {character.Character_Index}");

            await gameDb.Query(TableNames.UserCharacter)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, character.Character_Index)
                .UpdateAsync(new
                {
                    Is_Active = false,
                    Level = character.Level,
                    Exp = character.Exp
                });
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

    public async Task<Result> EquipItemAsync(Req_EquipItem requestBody)
    {
        // 요청한 아이템 보유 확인
        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
        UserEquip hasItem = await gameDb.Query(TableNames.UserInventory)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
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

        var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);
        Item itemInfo = await masterDb.Query(TableNames.InfoItem)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
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


        // 중복으로 활성화 되어있는 아이템 확인
        var otherEquippedItems = await gameDb.Query($"{TableNames.UserEquip} AS ue")
                .Join($"{TableNames.InfoItem} AS i", "ue.item_index", "i.item_index")
                .Where("ue.user_index", requestBody.UserIndex)
                .Where("ue.character_index", activatedCharacter.Character_Index)
                .Where("ue.is_equipped", true)
                .Where("i.type", itemInfo.Item_Type)
                .Where("ue.item_index", "!=", requestBody.EquipItemIndex)
                .GetAsync();

        foreach (var equipped in otherEquippedItems)
        {
            await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, equipped.item_index)
                .UpdateAsync(new { is_equipped = false });
        }

        var newEquipItem = await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                .FirstOrDefaultAsync();

        if (newEquipItem == null)
        {
            await gameDb.Query(TableNames.UserEquip).InsertAsync(new
            {
                user_index = requestBody.UserIndex,
                character_index = activatedCharacter.Character_Index,
                item_index = requestBody.EquipItemIndex,
                is_equipped = true
            });
        }
        else
        {
            await gameDb.Query(TableNames.UserEquip)
                .Where(DbColumns.UserIndex, requestBody.UserIndex)
                .Where(DbColumns.CharacterIndex, activatedCharacter.Character_Index)
                .Where(DbColumns.ItemIndex, requestBody.EquipItemIndex)
                .UpdateAsync(new { is_equipped = true });
        }

        return Result.Success();
    }


    public Task<Result<Res_Feed>> FeedAsync(Req_Feed requestBody)
        => UseGoodsAsync(requestBody.UserIndex, requestBody.GoodsIndex, requestBody.Quantity, _feedHandler);

    public Task<Result<Res_Play>> PlayAsync(Req_Play requestBody)
        => UseGoodsAsync(requestBody.UserIndex, requestBody.GoodsIndex, requestBody.Quantity, _playHandler);

    private async Task<Result<T>> UseGoodsAsync<T>(int userIndex, int goodsIndex, int quantity, IGoodsHandler<T> handler)
        where T : class, new()
    {
        // 요청한 goodsIndex가 먹이 타입인지 확인
        var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);
        Goods goods = await masterDb.Query(TableNames.InfoGoods)
                .Where(DbColumns.GoodsIndex, goodsIndex)
                .FirstOrDefaultAsync<Goods>();

        if (goods == null || (Enums.GoodType)goods.Type != handler.RequiredGoodType)
        {
            Console.WriteLine("잘못된 재화 타입 입니다.");
            return Result<T>.Fail(ResultCodes.Goods_Fail_NotValidType);
        }

        // 요청한 재화의 수량 확인
        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
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
