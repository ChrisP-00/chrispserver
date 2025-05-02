using chrispserver.ResReqModels;
using static chrispserver.ResReqModels.Request;
using static chrispserver.ResReqModels.Response;

namespace chrispserver.Services;

public interface IAccount
{

    // 계정 생성
    Task<Result<Res_Login>> CreateAccountAsync(Req_CreateAccount requestBody);


    // 계정 로그인
    Task<Result<Res_Login>> LoginAsync(Req_Login requestBody);

    // 계정 로그인 & 계정 생성 요청
    Task<Result<Res_Login>> LoginOrCreateAccountAsync(Req_Login requestBody);


    // 계정 삭제
  

    // 계정 블록

}
