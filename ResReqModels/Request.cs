using static chrispserver.ResReqModels.Enums;

namespace chrispserver.ResReqModels;


public class Request
{
    public interface IUserAuth
    {
        public int UserIndex { get; set; }
        public string DeviceId { get; set; }
    }

    public class Req_UserAuth : IUserAuth
    {
        public int UserIndex { get; set; }
        public string DeviceId { get; set; } = string.Empty;
    }


    #region 계정 관련 모델
    public class Req_CreateAccount
    {
        public string MemberId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? Nickname { get; set; }
    }

    public class Req_Login
    {
        public string MemberId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? Nickname { get; set; }
    }
    #endregion


    #region 캐릭터 관련 모델
    public class Req_PlayStatus : Req_UserAuth
    {
        public GoodType GoodType { get; set; }
        public int GoodsIndex { get; set; }
        public int Quantity { get; set; }
    }

    public class Req_Exp : Req_UserAuth
    {
        public int Exp { get; set; }
    }

    public class Req_EquipCharacter : Req_UserAuth
    {
        public int EquipCharacterIndex { get; set; }
    }

    public class Req_EquipItem : Req_UserAuth
    {
        public int EquipItemIndex { get; set; }
    }
    #endregion

    #region 미션 관련 모델
    public class Req_DailyMission : Req_UserAuth
    {
        public int DailyMissionIndex { get; set; }
        public int GoodsType { get; set; }
        public int GoodsIndex { get; set; }
        public int Amount { get; set; }
    }

    public class Req_ReceiveMission : Req_UserAuth
    {
        public int DailyMissionIndex { get; set; }
    }
    #endregion
}