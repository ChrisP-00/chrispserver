namespace chrispserver.DbEntity;

public class InfoEntities
{
    public class Define
    {
        public int Define_Index { get; set; }
        public string? Description { get; set; }
        public float Value { get; set; }
    }

    public class Goods
    {
        public int Goods_Index { get; set; }
        public int Type { get; set; }
        public string? Name { get; set; }
    }

    public class Character
    {
        public int Character_Index { get; set; }
        public string? Name { get; set; }
    }


    public class Item
    {
        public int Item_Index { get; set; }
        public string? Name { get; set; }
        public byte Type { get; set; }
        public int Required_Level { get; set; }
        public int Equip_Character_Index { get; set; }
    }


    public class DailyMission
    {
        public int Daily_Mission_Index { get; set; }
        public int Type { get; set; }
        public int Goods_Index { get; set; }
        public int Quantity { get; set; }
        public int Reward_Type { get; set; }
        public int Reward_Amount { get; set; }
    }

}
