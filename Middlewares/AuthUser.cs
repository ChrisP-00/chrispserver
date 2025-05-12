namespace chrispserver.Middlewares;

public class AuthUser
{
    public string? MemberId { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public bool IsGuest { get; set; } = false;
}
