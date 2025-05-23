﻿namespace chrispserver.DbEntity;

public class UserEntities
{
    public class UserAccount
    {
        public int User_Index { get; set; }
        public string? Member_Id { get; set; }
        public string? Device_Id { get; set; }
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
        public bool Is_Acquired { get; set; }
        public DateTime Equipped_At { get; set; }
        public DateTime Acquired_At { get; set; }
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
        public int Item_Type { get; set; }
        public int Item_Index { get; set; }
        public bool Is_Equipped { get; set; }
    }

    public class UserDailyMission
    {
        public int User_Index { get; set; }
        public int Daily_Mission_Index { get; set; }
        public int Mission_Goal_Count { get; set; }
        public int Mission_Progress { get; set; }
        public bool Is_Received { get; set; }
        public DateTime Updated_At { get; set; }
    }
}
