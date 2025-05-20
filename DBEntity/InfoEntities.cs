namespace chrispserver.DbEntity;

public class InfoEntities
{
    public class InfoDefine
    {
        public int Define_Index { get; set; }
        public string? Description { get; set; }
        public float Value { get; set; }
    }

    public class InfoGoods
    {
        public int Goods_Index { get; set; }
        public int Goods_Type { get; set; }
        public string? Name { get; set; }
    }

    public class InfoCharacter
    {
        public int Character_Index { get; set; }
        public string? Character_Name { get; set; }
    }


    public class InfoItem
    {
        public int Item_Index { get; set; }
        public string? Item_Name { get; set; }
        public byte Item_Type { get; set; }
        public int Required_Level { get; set; }
        public int Equip_Character_Index { get; set; }
    }


    public class InfoDailyMission
    {
        public int Daily_Mission_Index { get; set; }
        public int Goods_Type { get; set; }
        public int Goods_Index { get; set; }
        public byte Mission_Goal_Count { get; set; }
        public int Reward_Type { get; set; }
        public int Reward_Amount { get; set; }
    }


    public class InfoLevel
    {
        public int Level_Index { get; set; }
        public int Character_Index { get; set; }
        public int Level { get; set; }
        public int Required_Exp { get; set; }
    }
}
