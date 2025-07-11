﻿using chrispserver.Middlewares;
using StackExchange.Redis;
using System.Text.Json;
using static chrispserver.ResReqModels.Request;

namespace chrispserver.Securities;

public class RedisAuthService : IRedisAuthService
{
    private readonly IDatabase _db;
    private readonly int _ttlMinutes;
    private readonly TimeSpan _ttl;

    public RedisAuthService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        if(redis == null)
        {
            Console.WriteLine("Redis 연결 없음");
        }

        _db = redis.GetDatabase();
        _ttlMinutes = configuration.GetSection("Auth").GetValue<int>("TokenExpireMinutes", 100);
        _ttl = TimeSpan.FromMinutes(_ttlMinutes);
    }

    public async Task<string> GenerateTokenAsync(string memberId, string deviceId)
    {
        var oldToken = await _db.StringGetAsync(RedisKeyMaker.AuthUserKey(memberId));
        if (oldToken.HasValue)
        {
            await _db.KeyDeleteAsync(RedisKeyMaker.AuthTokenKey(oldToken.ToString()));
        }

        string token;
        do
        {
            token = Guid.NewGuid().ToString("N");
        } while (await _db.KeyExistsAsync(token));

        AuthUser authUser = new()
        {
            MemberId = memberId,
            AuthToken = token,
            DeviceId = deviceId,
        };

        // token → AuthUser (JSON 직렬화)
        await _db.StringSetAsync(
            RedisKeyMaker.AuthTokenKey(token),
             JsonSerializer.Serialize(authUser),
            _ttl
            );

        // memberId -> token 역방향 조회
        var success = await _db.StringSetAsync(
            RedisKeyMaker.AuthUserKey(memberId),
            token,
            _ttl
            );

        Console.WriteLine($"[Redis] Token 저장 성공 여부: {success}");

        return token;
    }

    public async Task<AuthUser?> GetAuthUserByTokenAsync(string token)
    {
        var json = await _db.StringGetAsync(RedisKeyMaker.AuthTokenKey(token));
        return json.HasValue ? JsonSerializer.Deserialize<AuthUser?>(json!) : null;
    }

    public async Task<string?> GetTokenByIdAsync(string id, bool isGuest)
    {
        var key = RedisKeyMaker.AuthUserKey(id);
        var token = await _db.StringGetAsync(key);
        return token.HasValue ? token.ToString() : null;
    }

    public async Task ExtendTTLAsync(AuthUser authUser)
    {
        await _db.KeyExpireAsync(RedisKeyMaker.AuthTokenKey(authUser.AuthToken), _ttl);
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        var authUser = await GetAuthUserByTokenAsync(token);
        if (authUser == null) return false;

        await _db.KeyDeleteAsync(RedisKeyMaker.AuthTokenKey(token));
        await _db.KeyDeleteAsync(RedisKeyMaker.AuthUserKey(authUser.MemberId!));

        return true;
    }

    public async Task<bool> ForceLogoutAsync(string token) => await RevokeTokenAsync(token);
}
