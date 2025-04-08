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
            [typeof(InfoItem)] = index => _master.Items.FirstOrDefault(i => i.Item_Index == index),
            [typeof(InfoGoods)] = index => _master.Goods.FirstOrDefault(g => g.Goods_Index == index),
            [typeof(InfoCharacter)] = index => _master.Characters.FirstOrDefault(c => c.Character_Index == index),
            [typeof(InfoDailyMission)] = index => _master.DailyMissions.FirstOrDefault(m => m.Daily_Mission_Index == index),
            [typeof(InfoDefine)] = index => _master.Defines.FirstOrDefault(d => d.Define_Index == index)
        };
    }

    public List<T>? GetAll<T>() where T : class
    {
        Console.WriteLine($"[MasterHandler] GetAll 호출: {typeof(T).Name}");

        if (_masterLists.TryGetValue(typeof(T), out var list))
        {
            if (typeof(T) == typeof(InfoDailyMission))
            {
                var actualList = list as List<InfoDailyMission>;
                Console.WriteLine($"[DEBUG] DailyMissions Count: {actualList?.Count}");
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
            [typeof(InfoItem)] = _master.Items,
            [typeof(InfoGoods)] = _master.Goods,
            [typeof(InfoCharacter)] = _master.Characters,
            [typeof(InfoDailyMission)] = _master.DailyMissions,
            [typeof(InfoDefine)] = _master.Defines,
        };
    }

}
