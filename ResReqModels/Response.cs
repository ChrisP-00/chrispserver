using static chrispserver.DBEntity.UserEntities;

namespace chrispserver.ResReqModels;


public class Response
{
    public class Res_Login
    {
        public UserAccount? userAccount { get; set; }
        public List<UserCharacter>? userCharacters { get; set; }
        public List<UserInventory>? userInventories { get; set; }
        public List<UserGoods>? userGoods { get; set; }
        public List<UserDailyMission>? userDailyMission { get; set; }
        public List<UserCharacterMission>? userCharacterMissions { get; set; }
    }

}
