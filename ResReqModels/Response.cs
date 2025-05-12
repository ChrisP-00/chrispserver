namespace chrispserver.ResReqModels;


public class Response
{
    #region 계정 관련 모델
    public class Res_Login
    {
        public string Token { get; set; } = string.Empty;
        public User_Account? UserAccount { get; set; }
        public List<User_Character>? UserCharacters { get; set; }
        public List<User_Inventory>? UserInventories { get; set; }
        public List<User_Equip>? UserEquips { get; set; }
        public List<User_Goods>? UserGoods { get; set; }
        public List<User_Daily_Missions>? UserDailyMission { get; set; }
    }
    #endregion

    public class User_Account
    {
        public string? Member_Id { get; set; }
        public string? Device_Id {  get; set; }
        public int User_Index { get; set; }
        public string? Nickname { get; set; }
        public bool Is_Banned { get; set; }
        public bool Is_Deleted { get; set; }
        public DateTime Last_Login_At { get; set; }
    }

    public class User_Character
    {
        public int Character_Index { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public bool Is_Active { get; set; }
        public bool is_acquired { get; set; }
    }

    public class User_Inventory
    {
        public int Item_Index { get; set; }
    }

    public class User_Equip
    {
        public int Character_Index { get; set; }
        public int Item_Type { get; set; }
        public int Item_Index { get; set; }
    }

    public class User_Goods
    {
        public int Goods_Index { get; set; }
        public int Quantity { get; set; }
    }

    public class User_Daily_Missions
    {
        public int Daily_Mission_Index { get; set; }
        public int Mission_Progress { get; set; }
        public bool Is_Received { get; set; }
    }

    #region 캐릭터 관련 모델
    public class Res_EquipCharacter
    {
        public int Level { get; set; }
        public int Exp { get; set; }
        public DateTime Equipped_at { get; set; } 
    }
    #endregion

    #region 미션 관련 모델
    public class Res_DailyMission
    {
        public int Daily_Mission_Index { get; set; }
        public int Mission_Progress { get; set; }
        public bool Is_Received { get; set; }
    }
    #endregion
}
