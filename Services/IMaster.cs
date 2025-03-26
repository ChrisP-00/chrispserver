namespace chrispserver.Services;
using static chrispserver.DBEntity.InfoEntities;

public interface IMaster
{
    Task LoadAllAsync();
    List<Character> Characters { get; }
    List<CharacterMission> CharacterMissions { get; }
    List<DailyMission> DailyMissions { get; }
    List<Define> Defines { get; }
    List<Goods> Goods { get; }
    List<Item> Items { get; }
}