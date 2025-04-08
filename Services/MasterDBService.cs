using chrispserver.DbConfigurations;
using SqlKata.Execution;
using static chrispserver.DbEntity.InfoEntities;
using static StackExchange.Redis.Role;

namespace chrispserver.Services;

public class MasterDBService : IMaster
{
    private readonly ConnectionManager _connectionManager;

    public List<InfoCharacter> Characters { get; private set; } = new();
    public List<InfoDailyMission> DailyMissions { get; private set; } = new();
    public List<InfoDefine> Defines { get; private set; } = new();
    public List<InfoGoods> Goods { get; private set; } = new();
    public List<InfoItem> Items { get; private set; } = new();

    public MasterDBService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task LoadAllAsync()
    {
        try
        {
            using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDataDB);

            Characters = (await db.Query(TableNames.InfoCharacter).GetAsync<InfoCharacter>()).ToList();
            DailyMissions = (await db.Query(TableNames.InfoDailyMission).GetAsync<InfoDailyMission>()).ToList();
            Defines = (await db.Query(TableNames.InfoDefine).GetAsync<InfoDefine>()).ToList();
            Goods = (await db.Query(TableNames.InfoGoods).GetAsync<InfoGoods>()).ToList();
            Items = (await db.Query(TableNames.InfoItem).GetAsync<InfoItem>()).ToList();

            foreach (var c in DailyMissions)
            {
                Console.WriteLine($"[DailyMissions] Index: {c.Daily_Mission_Index}");
            }
            Console.WriteLine($"[MasterDB] DailyMissions 로딩 수: {DailyMissions.Count}");

            Console.WriteLine("[MasterDB] 모든 마스터 데이터 로딩 완료");
        }
        catch (Exception ex)
        { 
            Console.WriteLine($"[MasterDB] 모든 마스터 데이터 로딩 실패 : {ex.ToString()}");
            throw;
        }
    }
}

