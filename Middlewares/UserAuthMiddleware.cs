using chrispserver.ResReqModels;
using chrispserver.Securities;
using System.Net;
using System.Text;
using System.Text.Json;

namespace chrispserver.Middlewares;

public class UserAuthMiddleware
{
    private readonly RequestDelegate _nextTask;
    private readonly IMemoryDb _memoryDB;
    private readonly IRedisAuthService _redisAuthService;

    public UserAuthMiddleware(RequestDelegate nextTask, IMemoryDb memoryDb, IRedisAuthService redisAuthService)
    {
        _nextTask = nextTask;
        _memoryDB = memoryDb;
        _redisAuthService = redisAuthService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // account/LoginOrCreateAccount 요청은 넘기기
        if (context.Request.Method != HttpMethods.Post ||
            context.Request.Path.Value?.StartsWith("/Account/LoginOrCreateAccount", StringComparison.OrdinalIgnoreCase) == true ||
            context.Request.Path.Value?.StartsWith("/Test/CheckMissionCount", StringComparison.OrdinalIgnoreCase) == true)
        {
            await _nextTask(context);
            return;
        }

        ParsedAuth? parsed = await ParseAuthAsync(context);
        if (parsed == null || !parsed.IsValid)
        {
            await RespondError(context, ResultCodes.AuthTokenFailWrongKeyword);
            return;
        }

        Console.WriteLine($"pased data: {parsed.Token}");
        Console.WriteLine($"pased data: {parsed.MemberId}");
        Console.WriteLine($"pased data: {parsed.DeviceId}");

        var authUser = await _redisAuthService.GetAuthUserByTokenAsync(parsed.Token);
        if (authUser == null)
        {
            await RespondError(context, ResultCodes.AuthTokenFailWrongUserAuthToken);
            return;
        }

        // 중복 로그인 확인 (유저만)
        if (authUser.MemberId != authUser.DeviceId && authUser.DeviceId != parsed.DeviceId)
        {
            await RespondError(context, ResultCodes.AuthTokenFailDuplicatedLogin);
            return;
        }

        await _redisAuthService.ExtendTTLAsync(authUser);

        var lockKey = MemoryDbKeyMaker.MakeUserLockKey(authUser.MemberId!);
        if (!await _memoryDB.SetUserReqLockAsync(lockKey))
        {
            await RespondError(context, ResultCodes.AuthTokenFailSetNx);
            return;
        }

        context.Items["AuthUser"] = authUser;

        try
        {
            await _nextTask(context);
        }
        finally
        {
            await _memoryDB.DelUserReqLockAsync(lockKey);
        }
    }


    private record ParsedAuth(string Token, string? MemberId, string DeviceId)
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(Token) &&
            !string.IsNullOrWhiteSpace(DeviceId);
    }

    private async Task<ParsedAuth?> ParseAuthAsync(HttpContext context)
    {
        string token = string.Empty;
        string? memberId = string.Empty;
        string deviceId = string.Empty;

        // Authorization 헤더 확인 (JWT 형식)
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var rawAuth = authHeader.ToString();
            if (!string.IsNullOrWhiteSpace(rawAuth))
                token = rawAuth.Replace("Bearer ", "").Trim();
        }

        if (context.Request.Headers.TryGetValue("X-Device-Id", out var deviceHeader))
        {
            var rawDevice = deviceHeader.ToString();
            if (!string.IsNullOrWhiteSpace(rawDevice))
                deviceId = rawDevice.Trim();
        }

        context.Request.EnableBuffering();
        context.Request.Body.Position = 0;

        // JSON Body로부터 ID, AuthToken 파싱
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(bodyStr))
            {
                try
                {
                    var doc = JsonDocument.Parse(bodyStr);

                    // 바디에 토큰이 있다면 덮어쓰기
                    if (doc.RootElement.TryGetProperty("AuthToken", out var tokenProp))
                        token = tokenProp.GetString()?.Trim() ?? token;

                    if (doc.RootElement.TryGetProperty("MemberId", out var mid))
                        memberId = mid.GetString()?.Trim() ?? "";

                    if (doc.RootElement.TryGetProperty("DeviceId", out var dev))
                        deviceId = dev.GetString()?.Trim() ?? deviceId;
                }
                catch
                {
                    return null;
                }
            }

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(deviceId))
                return null;

            return new ParsedAuth(token, memberId, deviceId);
        }
    }


    private AuthUser CreateAuthUser(string? memberId, string deviceId, string token, bool isGuest)
    {
        return new AuthUser
        {
            MemberId = memberId ?? "",
            DeviceId = deviceId,
            AuthToken = token,
        };
    }

    private async Task RespondError(HttpContext context, ResultCodes resultCode)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

        var response = JsonSerializer.Serialize(

            Result.Fail(resultCode)
        );

        var bytes = Encoding.UTF8.GetBytes(response);
        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
    }
}
