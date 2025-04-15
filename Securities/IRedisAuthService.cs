namespace chrispserver.Securities;

public interface IRedisAuthService
{
    Task<string> GenerateTokenAsync(string memberId);
    Task<string?> ValidateTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string token);
    Task ForceLogoutAsync(string memberId);
    Task<bool> ExtendTokenTTLAsync(string token);
}