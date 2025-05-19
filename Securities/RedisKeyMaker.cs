namespace chrispserver.Securities;

public class RedisKeyMaker
{
    // 공통
    // 키: auth:token:{token}, 값: "user:{deviceId}"
    public static string AuthTokenKey(string token) => $"auth:token:{token}";

    // 일반 유저용
    // 키: auth:user:{deviceId}: -> Token
    // 키: auth:user:{deviceId}:device -> deviceId
    public static string AuthUserKey(string deviceId) => $"auth:user:{deviceId}";

    // 유저 요청 락용
    public static string UserLockKey(string userId) => $"lock:{userId}";
}