using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user, Guid tenantId, string sessionId);
    (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid tenantId, string sessionId);
    TokenValidationResult? ValidateToken(string token);
}

public class TokenValidationDto
{
    public Guid UserId { get; set; }
    public Guid SessionId { get; set; }
    public Guid TenantId { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = "Cliente";
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

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
