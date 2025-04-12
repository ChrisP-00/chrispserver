using chrispserver.DbConfigurations;
using SqlKata.Execution;
using static chrispserver.DbEntity.InfoEntities;
using static StackExchange.Redis.Role;

namespace chrispserver.Services;

public class MasterDBService : IMaster
{
    private readonly ConnectionManager _connectionManager;

    public List<InfoCharacter> InfoCharacters { get; private set; } = new();
    public List<InfoDailyMission> InfoDailyMissions { get; private set; } = new();
    public List<InfoDefine> InfoDefines { get; private set; } = new();
    public List<InfoGoods> InfoGoods { get; private set; } = new();
    public List<InfoItem> InfoItems { get; private set; } = new();
    public List<InfoLevel> InfoLevels { get; private set; } = new();

    public MasterDBService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task LoadAllAsync()
    {
        try
        {
            using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);

            InfoCharacters = (await db.Query(TableNames.InfoCharacter).GetAsync<InfoCharacter>()).ToList();
            InfoDailyMissions = (await db.Query(TableNames.InfoDailyMission).GetAsync<InfoDailyMission>()).ToList();
            InfoDefines = (await db.Query(TableNames.InfoDefine).GetAsync<InfoDefine>()).ToList();
            InfoGoods = (await db.Query(TableNames.InfoGoods).GetAsync<InfoGoods>()).ToList();
            InfoItems = (await db.Query(TableNames.InfoItem).GetAsync<InfoItem>()).ToList();
            InfoLevels = (await db.Query(TableNames.InfoLevels).GetAsync<InfoLevel>()).ToList();

            foreach (var c in InfoLevels)
            {
                Console.WriteLine($"[InfoLevels] Index: {c.Level_Index}");
            }
            Console.WriteLine($"[MasterDB] InfoLevels 로딩 수: {InfoLevels.Count}");

            Console.WriteLine("[MasterDB] 모든 마스터 데이터 로딩 완료");
        }
        catch (Exception ex)
        { 
            Console.WriteLine($"[MasterDB] 모든 마스터 데이터 로딩 실패 : {ex.ToString()}");
            throw;
        }
    }
}

