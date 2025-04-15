using StackExchange.Redis;
using System.Text.Json;

namespace chrispserver.Middlewares;

public class RedisMemoryDb : IMemoryDb
{
    private readonly IDatabase _db;
    private const int LockExpireSeconds = 3;

    public RedisMemoryDb(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<(bool, AuthUser?)> GetUserAsync(string id)
    {
        var data = await _db.StringGetAsync($"user:{id}");
        if (!data.HasValue)
        {
            return (false, null);
        }

        return (true, JsonSerializer.Deserialize<AuthUser>(data!));
    }

    public async Task<bool> SetUserReqLockAsync(string token)
    {
        return await _db.StringSetAsync($"lock:{token}", "1", TimeSpan.FromSeconds(LockExpireSeconds), When.NotExists);
    }

    public async Task DelUserReqLockAsync(string lockKey)
    {
       await _db.KeyDeleteAsync(lockKey);
    }
}
