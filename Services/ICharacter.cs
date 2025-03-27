using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Services;

public interface ICharacter
{
    Task<Result<Res_Feed>> FeedAsync(Req_Feed requestBody);
    Task<Result<Res_Play>> PlayAsync(Req_Play requestBody);


    // 클라가 가지고 있는건 참고용 수량
    // 실제 유저가 가지고 있는 수량은 서버가 찐으로 보고 검증을 한 후에 차감

    // 클라에서 유저 인덱스, 굿즈 인덱스, 수량으로 요청 
    // 요청 시 행위와 재화 검토 후 서버로 요청 (밥주기 => 간식 타입 재화 사용) 

    // 서버에서 해당 유저의 재화 수량이 요청 수량보다 같거나 많은지 확인

    // 이상 없다면 차감 후 남은 횟수 반환

    // 요청했는데 먹이가 없는 경우 -> 오류

    // 먹이는 한번에 한번씩만 가능 <- 클라에서 처리
    // 처리 후 결과 메세지, 실제 남은 수량




}
