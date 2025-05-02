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
        Console.WriteLine("진입");

        if (context.Request.Method != HttpMethods.Post)
        {
            await _nextTask(context);
            return;
        }

        // account에서 create, login만 넘기기
        if (context.Request.Path.Value?.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) == true)
        {
            await _nextTask(context);
            return;
        }

        context.Request.EnableBuffering();

        string authToken = string.Empty;
        string memberId = string.Empty;
        string userLockKey = string.Empty;

        // Authorization 헤더 확인 (JWT 형식)
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            authToken = authHeader.ToString().Replace("Bearer ", "").Trim();

            if (!string.IsNullOrEmpty(authToken))
            {
                memberId = await _redisAuthService.ValidateTokenAsync(authToken);

                if (!string.IsNullOrEmpty(memberId))
                {
                    context.Items["MemberId"] = memberId;
                    await _redisAuthService.ExtendTokenTTLAsync(authToken);

                    userLockKey = await SetUserLockAsync(context, memberId);

                    context.Items["AuthUser"] = new AuthUser
                    {
                        MemberId = memberId,
                        AuthToken = authToken,
                    };
                    await Proceed(context, userLockKey);
                    return;
                }
            }
        }

        // JSON Body로부터 ID, AuthToken 파싱
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 4096, true))
        {
            var bodyStr = await reader.ReadToEndAsync();

            // Json Body가 유효한지 확인
            if (string.IsNullOrWhiteSpace(bodyStr))
            {
                await RespondError(context, ResultCodes.InValidRequestHttpBody);
                return;
            }

            // Json Body의 형식이 유효한지 확인
            if (!TryExtractJson(bodyStr, out memberId, out authToken))
            {
                await RespondError(context, ResultCodes.AuthTokenFailWrongKeyword);
                return;
            }

            // Redis에 해당 유저의 토큰이 있는지 확인
            var (isOk, userInfo) = await _memoryDB.GetUserAsync(memberId);
            if (!isOk || userInfo == null || userInfo.AuthToken != authToken)
            {
                // 토큰 만료 혹은 잘못된 토큰으로 클라에 반환
                await RespondError(context, ResultCodes.AuthTokenFailWrongAuthToken);
                return;
            }

            userLockKey = await SetUserLockAsync(context, userInfo.MemberId);

            context.Items["AuthUser"] = userInfo;
        }

        context.Request.Body.Position = 0;
        await Proceed(context, userLockKey);
    }

    private bool TryExtractJson(string body, out string id, out string authToken)
    {
        try
        {
            var document = JsonDocument.Parse(body);
            id = document.RootElement.GetProperty("ID").GetString() ?? "";
            authToken = document.RootElement.GetProperty("AuthToken").GetString() ?? "";
            return true;
        }
        catch
        {
            id = "";
            authToken = "";

            return false;
        }
    }

    private async Task<string> SetUserLockAsync(HttpContext context, string memberId)
    {
        string userLockKey = MemoryDbKeyMaker.MakeUserLockKey(memberId);

        if (!await _memoryDB.SetUserReqLockAsync(userLockKey))
        {
            Console.WriteLine($"[UserAuthMiddleware] 락 설정 시도 실패 ");
            await RespondError(context, ResultCodes.AuthTokenFailSetNx);
            return userLockKey;
        }

        Console.WriteLine($"[UserAuthMiddleware] 락 설정 시도 성공 {userLockKey}");

        return userLockKey;
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

    private async Task Proceed(HttpContext context, string userLockKey)
    {
        try
        {
            // 다음 컨트롤러로 요청 전달
            await _nextTask(context);
        }
        finally
        {
            // 완료 후 유저 락 제거
            if (!string.IsNullOrEmpty(userLockKey))
            {
                await _memoryDB.DelUserReqLockAsync(userLockKey);
            }
        }
    }
}
