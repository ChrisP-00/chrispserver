using chrispserver.Handlers;
using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Services;

public interface ICharacter
{
    Task<Result<Res_Feed>> FeedAsync(Req_Feed requestBody);
    
    Task<Result<Res_Play>> PlayAsync(Req_Play requestBody);

}
