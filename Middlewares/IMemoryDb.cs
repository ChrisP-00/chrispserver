namespace chrispserver.Middlewares;

public interface IMemoryDb
{
    Task<(bool, AuthUser?)> GetUserAsync(string id);
    Task<bool> SetUserReqLockAsync(string token);
    Task DelUserReqLockAsync(string lockKey);
}
