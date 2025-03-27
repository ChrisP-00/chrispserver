

using static chrispserver.ResReqModels.Enums;

namespace chrispserver.ResReqModels;


public class Request
{
    #region 계정 관련 모델
    public class Req_UserAuth
    {
        public int userIndex {  get; set; }
        public int userToken { get; set; }
    }

                
    public class Req_CreateAccount
    {
        public string? member_id { get; set; }
        public string? unity_device_number { get; set; }
        public string? nickname { get; set; }
    }

    public class Req_Login
    {
        public int member_id { get; set; }
        public string? unity_device_number { get; set; }
    }
    #endregion

    #region 캐릭터 관련 모델
    public class Req_Feed
    {
        public int user_index { get; set; }
        public int goods_index { get; set; }
        public int quantity { get; set; }
    }

    public class Req_Play
    {
        public int user_index { get; set; }
        public int goods_index { get; set; }
        public int quantity { get; set; }
    }
    #endregion
}
