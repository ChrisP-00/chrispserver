using chrispserver.DBConfigurations;
using SqlKata.Execution;
using static chrispserver.DBEntity.InfoEntities;

namespace chrispserver.Services;

public class MasterDBService : IMaster
{
    private readonly ConnectionManager _connectionManager;

    public List<Character> Characters { get; private set; } = new();
    public List<CharacterMission> CharacterMissions { get; private set; } = new();
    public List<DailyMission> DailyMissions { get; private set; } = new();
    public List<Define> Defines { get; private set; } = new();
    public List<Goods> Goods { get; private set; } = new();
    public List<Item> Items { get; private set; } = new();

    public MasterDBService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task LoadAllAsync()
    {
        var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);

        Characters = (await db.Query("character").GetAsync<Character>()).ToList();
        CharacterMissions = (await db.Query("character_mission").GetAsync<CharacterMission>()).ToList();
        DailyMissions = (await db.Query("daily_mission").GetAsync<DailyMission>()).ToList();
        Defines = (await db.Query("define").GetAsync<Define>()).ToList();
        Goods = (await db.Query("goods").GetAsync<Goods>()).ToList();
        Items = (await db.Query("item").GetAsync<Item>()).ToList();

        foreach (var c in Characters)
        {
            Console.WriteLine($"[Character] Index: {c.Character_Index}, Name: {c.Name}");
        }


        Console.WriteLine("[MasterDB] 모든 마스터 데이터 로딩 완료!");
    }
}

