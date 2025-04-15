namespace chrispserver.Middlewares;

public class AuthUser
{
    public string MemberId { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public int UserIndex { get; set; }
}
