using static chrispserver.DbEntity.InfoEntities;

namespace chrispserver.Services;

public class MasterHandler : IMasterHandler
{
    private readonly IMaster _master;
    private readonly Dictionary<Type, Func<int, object?>> _handlers;
    private readonly Dictionary<Type, object> _masterLists;

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

        _masterLists = new Dictionary<Type, object>
        {
            [typeof(InfoItem)] = _master.Items,
            [typeof(InfoGoods)] = _master.Goods,
            [typeof(InfoCharacter)] = _master.Characters,
            [typeof(InfoDailyMission)] = _master.DailyMissions,
            [typeof(InfoDefine)] = _master.Defines,
        };
    }

    public List<T> GetAll<T>() where T : class
    {
        if (_masterLists.TryGetValue(typeof(T), out var list))
            return ((List<T>)list);

        return new List<T>();
    }

    public T? GetInfoDataByIndex<T>(int index) where T : class
    {
        if (typeof(T) == typeof(InfoItem))
        {
            return _master.Items.FirstOrDefault(i => i.Item_Index == index) as T;
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
    }
}
