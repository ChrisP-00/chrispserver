using static chrispserver.DbEntity.InfoEntities;

namespace chrispserver.Services;

public class MasterHandler : IMasterHandler
{
    private readonly IMaster _master;
    private readonly Dictionary<Type, Func<int, object?>> _handlers;
    private Dictionary<Type, object> _masterLists = new();

    public MasterHandler(IMaster master)
    {
        _master = master;

        _handlers = new Dictionary<Type, Func<int, object?>>
        {
            [typeof(InfoItem)] = index => _master.InfoItems.FirstOrDefault(i => i.Item_Index == index),
            [typeof(InfoGoods)] = index => _master.InfoGoods.FirstOrDefault(g => g.Goods_Index == index),
            [typeof(InfoCharacter)] = index => _master.InfoCharacters.FirstOrDefault(c => c.Character_Index == index),
            [typeof(InfoDailyMission)] = index => _master.InfoDailyMissions.FirstOrDefault(m => m.Daily_Mission_Index == index),
            [typeof(InfoDefine)] = index => _master.InfoDefines.FirstOrDefault(d => d.Define_Index == index),
            [typeof(InfoLevel)] = index => _master.InfoLevels.FirstOrDefault(l => l.Level_Index == index)
        };
    }

    public List<T>? GetAll<T>() where T : class
    {
        Console.WriteLine($"[MasterHandler] GetAll 호출: {typeof(T).Name}");

        if (_masterLists.TryGetValue(typeof(T), out var list))
        {
            if (typeof(T) == typeof(InfoLevel))
            {
                var actualList = list as List<InfoLevel>;
            } 

            Console.WriteLine($"[MasterHandler] {typeof(T).Name} 가져오기 성공");
            return (list as List<T>);
        }

        Console.WriteLine($"[MasterHandler] {typeof(T).Name} 가져오기 실패 (딕셔너리에 없음)");
        return null;
    }

    public int GetDefaultValueOrDefault(int index, int defaultValue, string label)
    {
        var define = GetInfoDataByIndex<InfoDefine>(index);

        if (define == null)
        {
            Console.WriteLine($"[Define] {label} 누락 (Index: {index} -> 기본값 사용 : {defaultValue}");
            return defaultValue;
        }

        return (int)define.Value;
    }

    public T? GetInfoDataByIndex<T>(int index) where T : class
    {
        if (_handlers.TryGetValue(typeof(T), out var getter))
        {
            return getter(index) as T;
        }

        return null;
    }

    public bool IsValid<T>(int index) where T : class
    {
        return GetInfoDataByIndex<T>(index) != null;
    }

    public async Task LoadAllAsync()
    {
        await _master.LoadAllAsync();

        _masterLists.Clear();
        _masterLists = new Dictionary<Type, object>
        {
            [typeof(InfoItem)] = _master.InfoItems,
            [typeof(InfoGoods)] = _master.InfoGoods,
            [typeof(InfoCharacter)] = _master.InfoCharacters,
            [typeof(InfoDailyMission)] = _master.InfoDailyMissions,
            [typeof(InfoDefine)] = _master.InfoDefines,
            [typeof(InfoLevel)] = _master.InfoLevels,
        };
    }

    public InfoLevel? GetLevelInfo(int characterIndex, int level)
    {
        int customKey = characterIndex * 1000 + level;
        int globalKey = level;

        Console.WriteLine($"customKey: {customKey}");
        Console.WriteLine($"globalKey: {globalKey}");

        return GetInfoDataByIndex<InfoLevel>(customKey) ?? 
            GetInfoDataByIndex<InfoLevel>(globalKey);
    }

}
