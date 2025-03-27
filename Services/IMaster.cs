namespace chrispserver.Services;
using static chrispserver.DbEntity.InfoEntities;

public interface IMaster
{
    Task LoadAllAsync();
    List<Character> Characters { get; }
    List<DailyMission> DailyMissions { get; }
    List<Define> Defines { get; }
    List<Goods> Goods { get; }
    List<Item> Items { get; }
}