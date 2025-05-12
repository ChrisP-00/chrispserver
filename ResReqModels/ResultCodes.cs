using System.ComponentModel;

namespace chrispserver.ResReqModels;

// 통신 코드 정리
public enum ResultCodes
{
    [Description("성공")]
    Ok = 0,

    // 1 ~ 9
    [Description("필수 입력 필드 누락")]
    InputData_MissingRequiredField = 0001,

    // 트렌젝션 오류 -1000 ~ -1099
    Transaction_Fail_Rollback = -1001,
    Transaction_Fail_ConnectionStringNull = -1002,

    // 미들웨어 오류 -1100 ~ -1199
    [Description("중복 요청")]
    AuthTokenFailSetNx = -1101,

    [Description("유효하지 않은 요청")]
    InValidRequestHttpBody,

    [Description("Auth Token Fail")]
    AuthTokenFailWrongKeyword,

    [Description("잘못된 유저 Auth Token")]
    AuthTokenFailWrongUserAuthToken,

    [Description("잘못된 게스트 Auth Token")]
    AuthTokenFailWrongGuestAuthToken,

    [Description("중복된 로그인")]
    AuthTokenFailDuplicatedLogin,

    // 계정 오류 -2000 ~ -2499
    [Description("계정이 중복됨")]
    Create_Account_Fail_Duplicate = -2001,
    [Description("밴 계정")]
    Ban_Account = -2002,
    [Description("삭제된 계정")]
    Deleted_Account = -2003,
    [Description("맴버 아이디가 없음")]
    No_MemberId = -2004,
    [Description("게스트 계정 없음")]
    No_Guest_Account = -2005,
    [Description("게스트 계정 로그인 중 오류 발생")]
    Account_GuestLogin_Fail = -2006,
    [Description("게스트 계정에서 정식 계정으로 업데이트 중 오류 발생")]
    Account_Update_Fail = -2007,

    [Description("계정 생성 후 데이터 오류")]
    Create_Account_Fail = -2101,
    [Description("계정 생성 중 데이터 테이블 오류 발생")]
    Create_Account_Insert_Data_Fail = -2102,
    [Description("계정 생성 중 오류 발생")]
    Create_Account_Fail_Exception = -2103,

    [Description("수정 요청한 계정이 없음")]
    Update_Account_NoAccount = -2201,
    [Description("기존 닉네임과 같음")]
    Update_Account_SameNickname = -2202,
    [Description("특수문자가 포함되었거나 15자 이상")]
    Update_Account_InvalidNickname = -2203,
    [Description("계정 수정 중 오류 발생")]
    Update_Account_Fail_Exception = -2299,


    // 로그인 오류 -2500 ~ -2999
    [Description("해당 유저를 찾을 수 없음")]
    Login_Fail_NotUser = -2501,
    [Description("로그인 중 오류 발생")]
    Login_Fail_Exception = -2509,


    [Description("로그아웃 중 오류 발생")]
    Logout_Fail_Exception = -2999,



    // 캐릭터 상태 업데이트 오류 -3000 ~ -3499
    [Description("캐릭터 상태 업데이트 오류")]
    PlayStatus_Fail_Exception = -3499,

    // 재화 사용 오류 -3500 ~ -3999
    [Description("보유한 재화 수량이 부족함")]
    Goods_Fail_NotEnough = -3501,  
    [Description("사용자가 보유한 재화가 없음")]
    Goods_Fail_NotExist = -3502,  
    [Description("사용할 수 없는 재화")]
    Goods_Fail_NotValidType = -3503,
    [Description("보유한 재화 수량이 부족함")]
    Goods_Fail_LessThanZero = -3504,
    [Description("밥주기 중 오류 발생")]
    Goods_Fail_Exception = -3999,



    // 경험치 오류 -4000 ~ -4999
    [Description("존재하지 않는 캐릭터")]
    EXP_Fail_CharacterNotExist = 4009,
    EXP_Fail_Exception = 4999,

    // 미션 오류 -5000 ~ -5999
    [Description("현재 미션을 수행 할 수 없음")]
    Mission_Fail_NotAvailable = -5001,    
    [Description("이미 수행한 미션")]
    Mission_Fail_AlreadyCompleted = -5002,
    [Description("이미 수행한 미션")]
    Mission_Fail_NoMission = 5003,
    [Description("미션 알 수 없는 오류")]
    Mission_Fail_Exception = -5999,

    // 장착 오류 -6000 ~ -6999
    // 캐릭터 장착 오류 -6100 ~ -6200
    [Description("이미 장착된 캐릭터")]
    Equip_Fail_CharacterAlreadyEquipped = -6101,
    [Description("이미 장착된 캐릭터")]
    Equip_Fail_NoCharacter = -6102,
    [Description("존재하지 않는 캐릭터")]
    Equip_Fail_CharacterNotExist,

    // 아이템 장착 오류 -6200 ~ -6300
    [Description("장착할 아이템을 가지고 있지 않음")]
    Equip_Fail_NoItem = -6201,            
    [Description("이미 장착하고 있음")]
    Equip_Fail_ItemAlreadyEquipped = -6202,    
    [Description("장착 할 수 없는 아이템")]
    Equip_Fail_NoEquippedItem = -6203,
    [Description("장착 할 수 없는 아이템")]
    Equip_Fail_Incompatible = -6204,
    [Description("존재 하지 않는 아이템")]
    Equip_Fail_NotExist = -6291,
    [Description("아이템 알 수 없는 오류")]
    Equip_Fail_Exception = -6299,         


    // 공지 오류 -9000 ~ -9999
    [Description("요청한 공지가 없음")]
    Notice_Fail_NoNotice = -9001,         
    [Description("공지 생성 / 수정 / 삭제 / 권한 없음")]
    Notice_Fail_PermissionDenied = -9002, 
    [Description("공지 내용이 유효하지 않음")]
    Notice_Fail_InvalidContent = -9003,  
    [Description("동일한 공지가 존재함")]
    Notice_Fail_AlreadyExists = -9004,   
    [Description("공지가 중복 등록됨")]
    Notice_Fail_Duplicate = -905,       
    [Description("DB 업데이트 오류")]
    Notice_Fail_UpdateFail = -9006,      
    [Description("공지 알 수 없는 오류")]
    Notice_Fail_Exception = -9999,       


}