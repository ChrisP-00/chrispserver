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
            var charactersTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoCharacter).GetAsync<InfoCharacter>();
            });

            var missionsTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoDailyMission).GetAsync<InfoDailyMission>();
            });

            var definesTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoDefine).GetAsync<InfoDefine>();
            });

            var goodsTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoGoods).GetAsync<InfoGoods>();
            });

            var itemsTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoItem).GetAsync<InfoItem>();
            });

            var levelsTask = Task.Run(async () =>
            {
                using var db = _connectionManager.GetSqlQueryFactory(DbKeys.MasterDB);
                return await db.Query(TableNames.InfoLevels).GetAsync<InfoLevel>();
            });

            await Task.WhenAll(charactersTask, missionsTask, definesTask, goodsTask, itemsTask, levelsTask);

            InfoCharacters = (await charactersTask).ToList();
            InfoDailyMissions = (await missionsTask).ToList();
            InfoDefines = (await definesTask).ToList();
            InfoGoods = (await goodsTask).ToList();
            InfoItems = (await itemsTask).ToList();
            InfoLevels = (await levelsTask).ToList();

            foreach (var c in InfoLevels)
            {
                Console.WriteLine($"[InfoLevels] Index: {c.Level_Index}");
            }
            Console.WriteLine($"[MasterDB] InfoLevels 로딩 수: {InfoLevels.Count}");
            Console.WriteLine("[MasterDB] 모든 마스터 데이터 로딩 완료");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MasterDB] 마스터 데이터 로딩 실패 : {ex}");
            throw;
        }
    }

}

