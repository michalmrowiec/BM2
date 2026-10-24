namespace BM2.Infrastructure;

public class AuthenticationSettings
{
    public string JwtKey { get; set; } = null!;
    public int JwtExpireMinutes { get; set; }
    public string JwtIssuer { get; set; } = null!;
}