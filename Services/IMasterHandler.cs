namespace chrispserver.Services;

public interface IMasterHandler
{
    T? GetInfoDataByIndex<T>(int index) where T : class;

    bool IsValid<T>(int index) where T : class;

    List<T> GetAll<T>() where T : class;

    Task LoadAllAsync();
}
