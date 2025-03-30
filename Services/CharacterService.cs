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

    public Task<Result<Res_Feed>> FeedAsync(Req_Feed request)
        => UseGoodsAsync(request.Userindex, request.GoodsIndex, request.Quantity, _feedHandler);
    
    public Task<Result<Res_Play>> PlayAsync(Req_Play request)
        => UseGoodsAsync(request.Userindex, request.GoodsIndex, request.Quantity, _playHandler);
    
    private async Task<Result<T>> UseGoodsAsync<T>(int userIndex, int goodsIndex, int quantity, IGoodsHandler<T> handler)
        where T : class, new()
    {
        // 요청한 goodsIndex가 먹이 타입인지 확인
        var masterDb = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);
        Goods goods = await masterDb.Query(TableNames.Goods)
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
    
        if(userGoods == null)
        {
            Console.WriteLine("요청한 재화를 찾을 수 없습니다.");
            return Result<T>.Fail(ResultCodes.Goods_Fail_NotExist);
        }
        
        if(userGoods.Quantity < quantity)
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
