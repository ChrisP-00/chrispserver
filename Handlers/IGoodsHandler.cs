using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Handlers;

public interface IGoodsHandler<T> where T : class, new()
{
    Enums.GoodType RequiredGoodType { get; }

    T CreateResponse(int remainQuantity);
    
    // 재화 사용 후 효과 처리
}

public class FeedHandler : IGoodsHandler<Res_Feed>
{
    public Enums.GoodType RequiredGoodType => Enums.GoodType.Food;
    
    public Res_Feed CreateResponse(int remainQuantity)
    => new Res_Feed {RemainQuantity = remainQuantity};
}

public class PlayHandler : IGoodsHandler<Res_Play>
{
    public Enums.GoodType RequiredGoodType => Enums.GoodType.Toy;
    
    public Res_Play CreateResponse(int remainQuantity)
    => new Res_Play {RemainQuantity = remainQuantity};
}