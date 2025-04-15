namespace chrispserver.Securities;

public class RedisKeyMaker
{
    public static string MemberTokenKey(string memberId) => $"member:{memberId}:token";
    public static string TokenKey(string token) => $"token:{token}";
    public static string UserLockKey(string userId) => $"lock:{userId}";
}
