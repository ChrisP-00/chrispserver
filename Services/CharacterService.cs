using chrispserver.DbConfigurations;
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

    public CharacterService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<Result<Res_Feed>> FeedAsync(Req_Feed requestBody)
    {
        // 요청한 재화가 먹이 타입인지 확인
        var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);
        Goods goods = await masterDb.Query(TableNames.Goods)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .FirstOrDefaultAsync<Goods>();

        if ((Enums.GoodType)goods.Type != Enums.GoodType.Food)
        {
            Console.WriteLine($"{(Enums.GoodType)goods.Type} / {goods.Name}은 먹이로 사용할 수 없는 재화입니다.");
            return Result<Res_Feed>.Fail(ResultCodes.Feed_Fail_NotFood);
        }

        // 요청한 재화의 수량 확인
        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
        UserGoods userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.user_index)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .FirstOrDefaultAsync<UserGoods>();

        if(userGoods == null)
        {
            Console.WriteLine("요청한 재화를 찾을 수 없습니다.");
            return Result<Res_Feed>.Fail(ResultCodes.Feed_Fail_NotEnough);
        }

        int remainQuantity = userGoods.Quantity - requestBody.quantity;

        if(userGoods.Quantity < requestBody.quantity || remainQuantity < 0)
        {
            Console.WriteLine("요청한 재화의 수량이 부족합니다.");
            return Result<Res_Feed>.Fail(ResultCodes.Feed_Fail_NotEnough);
        }

        await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.user_index)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .UpdateAsync(new { quantity = remainQuantity });

        Res_Feed res_Feed = new Res_Feed
        {
            RemainQuantity = remainQuantity
        };

        return Result<Res_Feed>.Success(res_Feed);
    }

    public async Task<Result<Res_Play>> PlayAsync(Req_Play requestBody)
    {
        // 요청한 재화가 장난감 타입인지 확인
        var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);
        Goods goods = await masterDb.Query(TableNames.Goods)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .FirstOrDefaultAsync<Goods>();

        if ((Enums.GoodType)goods.Type != Enums.GoodType.Toy)
        {
            Console.WriteLine($"{(Enums.GoodType)goods.Type} / {goods.Name}은 놀이로 사용할 수 없는 재화입니다.");
            return Result<Res_Play>.Fail(ResultCodes.Play_Fail_NotToy);
        }

        // 요청한 재화의 수량 확인
        var gameDb = _connectionManager.GetSqlQueryFactory(DbKeys.GameServerDB);
        UserGoods userGoods = await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.user_index)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .FirstOrDefaultAsync<UserGoods>();

        if (userGoods == null)
        {
            Console.WriteLine("요청한 재화를 찾을 수 없습니다.");
            return Result<Res_Play>.Fail(ResultCodes.Play_Fail_NotEnough);
        }

        int remainQuantity = userGoods.Quantity - requestBody.quantity;

        if (userGoods.Quantity < requestBody.quantity || remainQuantity < 0)
        {
            Console.WriteLine("요청한 재화의 수량이 부족합니다.");
            return Result<Res_Play>.Fail(ResultCodes.Play_Fail_NotEnough);
        }

        await gameDb.Query(TableNames.UserGoods)
                .Where(DbColumns.UserIndex, requestBody.user_index)
                .Where(DbColumns.GoodsIndex, requestBody.goods_index)
                .UpdateAsync(new { quantity = remainQuantity });

        Res_Play res_Feed = new Res_Play
        {
            RemainQuantity = remainQuantity
        };

        return Result<Res_Play>.Success(res_Feed);
    }
}
