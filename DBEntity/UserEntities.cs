namespace chrispserver.DbEntity;

public class UserEntities
{
    public class UserAccount
    {
        public int User_Index { get; set; }
        public string? Member_id { get; set; }
        public string? Unity_device_number { get; set; }
        public string? Nickname { get; set; }
        public bool Is_Banned { get; set; }
        public bool Is_Deleted { get; set; }
        public DateTime Last_Login_At { get; set; }
    }


    public class UserCharacter
    {
        public int User_Index { get; set; }
        public int Character_Index { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public bool Is_Active { get; set; }
        public bool is_acquired { get; set; }
        public DateTime equipped_at { get; set; }
        public DateTime acquired_at { get; set; }
    }


    public class UserGoods
    {
        public int User_Index { get; set; }
        public int Goods_Index { get; set; }
        public int Quantity { get; set; }
    }

    public class UserInventory
    {
        public int User_Index { get; set; }
        public int Item_Index { get; set; }
    }


    public class UserEquip
    {
        public int User_Index { get; set; }
        public int Character_Index { get; set; }
        public int Item_Index { get; set; }
        public bool Is_Equipped { get; set; }
    }

    public class UserDailyMission
    {
        public int User_Index { get; set; }
        public int Mission_Index { get; set; }
        public int Quantity { get; set; }
        public bool Is_Received { get; set; }
    }
}
