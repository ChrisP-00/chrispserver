namespace chrispserver.Services;
using static chrispserver.DbEntity.InfoEntities;

public interface IMaster
{
    Task LoadAllAsync();
    List<InfoCharacter> Characters { get; }
    List<InfoDailyMission> DailyMissions { get; }
    List<InfoDefine> Defines { get; }
    List<InfoGoods> Goods { get; }
    List<InfoItem> Items { get; }
}