namespace BA.Backend.Infrastructure.Services;

public class TokenValidationResult
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Cliente";
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
