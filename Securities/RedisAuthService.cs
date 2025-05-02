using StackExchange.Redis;

namespace chrispserver.Securities;

public class RedisAuthService : IRedisAuthService
{
    private readonly IDatabase _db;
    private readonly int _expireMinutes;

    public RedisAuthService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _db = redis.GetDatabase();
        _expireMinutes = configuration.GetSection("Auth").GetValue<int>("TokenExpireMinutes", 1);
    }

    public async Task<string> GenerateTokenAsync(string memberId)
    {
        var oldToken = await _db.StringGetAsync(RedisKeyMaker.MemberTokenKey(memberId));
        if (oldToken.HasValue) 
        {
            await _db.KeyDeleteAsync(oldToken.ToString());
        }

        string token;
        do
        {
            token = Guid.NewGuid().ToString("N");
        } while (await _db.KeyExistsAsync(token));

        await _db.StringSetAsync(
            RedisKeyMaker.TokenKey(token),
            memberId,
            TimeSpan.FromMinutes(_expireMinutes)
            );

        await _db.StringSetAsync(
            RedisKeyMaker.MemberTokenKey(memberId),
            token,
            TimeSpan.FromMinutes(_expireMinutes)
            );

        return token;
    }

    public async Task<string?> ValidateTokenAsync(string token)
    {
        var value = await _db.StringGetAsync(RedisKeyMaker.TokenKey(token));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var redisValue = await _db.StringGetAsync(RedisKeyMaker.TokenKey(token));
        if (!redisValue.HasValue)
        {
            return false;
        }

        string memberId = redisValue.ToString();
        await _db.KeyDeleteAsync(RedisKeyMaker.TokenKey(token));
        await _db.KeyDeleteAsync(RedisKeyMaker.MemberTokenKey(memberId));
        return true;
    }

    public async Task ForceLogoutAsync(string memberId)
    {
        var redisValue = await _db.StringGetAsync(RedisKeyMaker.MemberTokenKey(memberId));
        if (redisValue.HasValue)
        {
            string token = redisValue.ToString();
            await _db.KeyDeleteAsync(RedisKeyMaker.TokenKey(token));
            await _db.KeyDeleteAsync(RedisKeyMaker.MemberTokenKey(memberId));
        }
    }

    public async Task<bool> ExtendTokenTTLAsync(string token)
    {
        var memberId = await _db.StringGetAsync(RedisKeyMaker.TokenKey(token));
        if (memberId.IsNullOrEmpty)
        {
            return false;
        }

        await _db.KeyExpireAsync(RedisKeyMaker.TokenKey(token), TimeSpan.FromMinutes(_expireMinutes));
        await _db.KeyExpireAsync(RedisKeyMaker.MemberTokenKey(memberId!), TimeSpan.FromMinutes(_expireMinutes));
        return true;
    }
}
