using chrispserver.Securities;

namespace chrispserver.Middlewares;

public interface IMemoryDb
{
    Task<bool> SetUserReqLockAsync(string token);
    Task DelUserReqLockAsync(string lockKey);
}