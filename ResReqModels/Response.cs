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
        public List<UserCharacterMission>? UserCharacterMissions { get; set; }
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
    #endregion
}
