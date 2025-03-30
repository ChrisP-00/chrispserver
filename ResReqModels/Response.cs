using static chrispserver.DbEntity.UserEntities;

namespace chrispserver.ResReqModels;


public class Response
{
    #region 계정 관련 모델
    public class Res_Login
    {
        public UserAccount? UserAccount { get; set; }
        public List<UserCharacter>? UserCharacters { get; set; }
        public List<UserInventory>? UserInventories { get; set; }
        public List<UserGoods>? UserGoods { get; set; }
        public List<UserDailyMission>? UserDailyMission { get; set; }
    }
    #endregion


    #region 캐릭터 관련 모델
    public class Res_Feed
    {
        public int RemainQuantity { get; set; }
    }

    public class Res_Play
    {
        public int RemainQuantity { get; set; }
    }

    public class Res_EquipCharacter
    {
        public int Level { get; set; }
        public int Exp { get; set; }
        public DateTime Equipped_at { get; set; } 

         // 장착한 아이템에 대한 정보 전달
         // public List<itemEquip> 
    }
    #endregion
}
