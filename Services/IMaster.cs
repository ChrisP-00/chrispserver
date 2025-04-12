namespace chrispserver.Services;
using static chrispserver.DbEntity.InfoEntities;

public interface IMaster
{
    Task LoadAllAsync();
    List<InfoCharacter> InfoCharacters { get; }
    List<InfoDailyMission> InfoDailyMissions { get; }
    List<InfoDefine> InfoDefines { get; }
    List<InfoGoods> InfoGoods { get; }
    List<InfoItem> InfoItems { get; }
    List<InfoLevel> InfoLevels { get; }
}