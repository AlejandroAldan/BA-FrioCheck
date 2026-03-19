namespace BA.Backend.Infrastructure.Settings;

public class JwtSettings
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = "BA.Backend.API";
    public string Audience { get; set; } = "BA.Backend.Clients";
    public int ExpirationMinutes { get; set; } = 15;
}
