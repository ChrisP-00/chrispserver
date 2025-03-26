using System.ComponentModel.DataAnnotations;

namespace chrispserver.ResReqModels;


public class Request
{
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
}
