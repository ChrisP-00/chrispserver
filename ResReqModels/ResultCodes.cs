using System.ComponentModel;

namespace chrispserver.ResReqModels;

// 통신 코드 정리
public enum ResultCodes
{
    [Description("성공")]
    Ok = 0,

    // 1 ~ 9
    [Description("필수 입력 필드 누락")]
    InputData_MissingRequiredField = 1,





    // 계정 오류 -100 ~ -199
    [Description("계정이 중복됨")]
    Create_Account_Fail_Duplicate = -100,
    [Description("밴 계정")]
    Ban_Account = -101,

    Deleted_Account = -102,

    [Description("계정 생성 후 데이터 오류")]
    Create_Account_Fail = -117,
    [Description("계정 생성 중 데이터 테이블 오류 발생")]
    Create_Account_Insert_Data_Fail = -118,
    [Description("계정 생성 중 오류 발생")]
    Create_Account_Fail_Exception = -119,




    [Description("수정 요청한 계정이 없음")]
    Update_Account_NoAccount = -110,

    [Description("기존 닉네임과 같음")]
    Update_Account_SameNickname = -115,
    [Description("특수문자가 포함되었거나 15자 이상")]
    Update_Account_InvalidNickname = -116,


    [Description("계정 수정 중 오류 발생")]
    Update_Account_Fail_Exception = -129,


    // 로그인 오류 -200 ~ -299
    [Description("해당 유저를 찾을 수 없음")]
    Login_Fail_NotUser = -200,
    [Description("로그인 중 오류 발생")]
    Login_Fail_Exception = -299,         // 로그인 알 수 없는 오류

    // 밥주기 오류 -300 ~ -399
    [Description("사용자가 보유한 음식이 없음")]
    Feed_Fail_NoFood = -300,             // 사용자가 보유한 음식이 없음
    [Description("밥을 먹을 수 없는 상태")]
    Feed_Fail_NotHungry = -301,          // 밥을 먹을 수 없는 상태
    [Description("밥주기 중 오류 발생")]
    Feed_Fail_Exception = -399,          // 밥주기 알 수 없는 오류

    // 놀아주기 오류 -400 ~ -499
    [Description("사용자가 보유한 장난감이 없음")]
    Play_Fail_NoToy = -400,              // 사용자가 보유한 장난감이 없음
    [Description("놀지 못하는 상태")]
    Play_Fail_NotInMood = -401,          // 놀지 못하는 상태
    [Description("놀아주기 중 오류 발생")]
    Play_Fail_Exception = -499,          // 놀아주기 알 수 없는 오류

    // 미션 오류 -500 ~ -599
    [Description("현재 미션을 수행 할 수 없음")]
    Mission_Fail_NotAvailable = -501,    // 현재 미션을 수행 할 수 없음
    [Description("이미 수행한 미션")]
    Mission_Fail_AlreadyCompleted = -502,   // 이미 수행한 미션
    [Description("미션 알 수 없는 오류")]
    Mission_Fail_Exception = -599,       // 미션 알 수 없는 오류

    // 장착 오류 -600 ~ -699
    [Description("장착할 아이템을 가지고 있지 않음")]
    Equip_Fail_NoItem = -600,            // 장착할 아이템을 가지고 있지 않음
    [Description("이미 장착하고 있음")]
    Equip_Fail_AlreadyEquiped = -601,    // 이미 장착하고 있음
    [Description("장착 할 수 없는 아이템")]
    Equip_Fail_Incompatible = -602,      // 장착 할 수 없는 아이템 
    [Description("아이템 알 수 없는 오류")]
    Equip_Fail_Exception = -699,         // 아이템 알 수 없는 오류

    // 뽑기 오류 -700 ~ -799
    [Description("뽑기에 필요한 재화 부족")]
    Gacha_Fail_NoCurrency = -700,        // 뽑기에 필요한 재화 부족
    [Description("뽑기 횟수 부족")]
    Gacha_Fail_NoAttempts = -701,        // 뽑기 횟수 부족
    [Description("뽑기 알 수 없는 오류")]
    Gacha_Fail_Exception = -799,         // 뽑기 알 수 없는 오류

    // 재화 오류 -800 ~ -899
    [Description("필요한 재화가 부족함")]
    Currency_Fail_NotEnough = -800,      // 필요한 재화가 부족함
    [Description("유효하지 않은 거래")]
    Currency_Fail_InValidTransaction = -801, // 유효하지 않은 거래
    [Description("재화 알 수 없는 오류")]
    Currency_Fail_Exception = -899,       // 재화 알 수 없는 오류

    // 공지 오류 -900 ~ -999
    [Description("요청한 공지가 없음")]
    Notice_Fail_NoNotice = -900,         // 요청한 공지가 없음
    [Description("공지 생성 / 수정 / 삭제 / 권한 없음")]
    Notice_Fail_PermissionDenied = -901, // 공지 생성 / 수정 / 삭제 / 권한 없음
    [Description("공지 내용이 유효하지 않음")]
    Notice_Fail_InvalidContent = -902,   // 공지 내용이 유효하지 않음
    [Description("동일한 공지가 존재함")]
    Notice_Fail_AlreadyExists = -903,    // 동일한 공지가 존재함
    [Description("공지가 중복 등록됨")]
    Notice_Fail_Duplicate = -904,        // 중복 등록됨
    [Description("DB 업데이트 오류")]
    Notice_Fail_UpdateFail = -905,        // DB 업데이트 오류
    [Description("공지 알 수 없는 오류")]
    Notice_Fail_Exception = -999,         // 공지 알 수 없는 오류


    // 트렌젝션 오류 -1000 ~ -1100

    Transaction_Fail_Rollback = -1001,
    Transaction_Fail_ConnectionStringNull = -1002,

}