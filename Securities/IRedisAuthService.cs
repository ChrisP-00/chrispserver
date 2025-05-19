using chrispserver.Middlewares;

namespace chrispserver.Securities;

public interface IRedisAuthService
{
    Task<string> GenerateTokenAsync(string memberId, string deviceId);

    Task<AuthUser?> GetAuthUserByTokenAsync(string token);

    Task<string?> GetTokenByIdAsync(string id, bool isGuest);

    Task ExtendTTLAsync(AuthUser authUser);

    Task<bool> RevokeTokenAsync(string token);

    Task<bool> ForceLogoutAsync(string token);
}