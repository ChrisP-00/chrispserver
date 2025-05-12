using chrispserver.ResReqModels;
using chrispserver.Securities;
using StackExchange.Redis;
using System.Text.Json;
using static Humanizer.In;

namespace chrispserver.Middlewares;

public class RedisMemoryDb : IMemoryDb
{
    private readonly IDatabase _db;
    private const int LockExpireSeconds = 3;

    public RedisMemoryDb(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<bool> SetUserReqLockAsync(string token)
    {
        return await _db.StringSetAsync(MemoryDbKeyMaker.MakeUserLockKey(token), "1", TimeSpan.FromSeconds(LockExpireSeconds), When.NotExists);
    }

    public async Task DelUserReqLockAsync(string lockKey)
    {
        await _db.KeyDeleteAsync(lockKey);
    }
}
