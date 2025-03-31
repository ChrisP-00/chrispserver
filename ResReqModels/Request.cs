

using static chrispserver.ResReqModels.Enums;

namespace chrispserver.ResReqModels;


public class Request
{
    public class Req_Base
    {
        public int UserIndex { get; set; }
    }


    #region 계정 관련 모델
    public class Req_UserAuth : Req_Base
    {
        public int UserToken { get; set; }
    }
     
    public class Req_CreateAccount
    {
        public string? MemberId { get; set; }
        public string? UnityDeviceNumber { get; set; }
        public string? Nickname { get; set; }
    }

    public class Req_Login
    {
        public int MemberId { get; set; }
        public string? UnityDeviceNumber { get; set; }
    }
    #endregion


    #region 캐릭터 관련 모델
    public class Req_Feed : Req_Base
    {
        public int GoodsIndex { get; set; }
        public int Quantity { get; set; }
    }

    public class Req_Play : Req_Base
    {
        public int GoodsIndex { get; set; }
        public int Quantity { get; set; }
    }

    public class Req_EquipCharacter : Req_Base
    {
        public int CharacterIndex { get; set; }
    }

    public class Req_EquipItem : Req_EquipCharacter
    {
        public int EquipItemIndex { get; set; }

    }
    #endregion
}
