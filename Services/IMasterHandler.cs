using static chrispserver.DbEntity.InfoEntities;

namespace chrispserver.Services;

public interface IMasterHandler
{
    T? GetInfoDataByIndex<T>(int index) where T : class;

    int GetDefaultValueOrDefault(int index, int defaultValue, string label);

    bool IsValid<T>(int index) where T : class;

    List<T>? GetAll<T>() where T : class;

    Task LoadAllAsync();

    InfoLevel? GetLevelInfo(int characterIndex, int level);
}
