using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Settings;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BA.Backend.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public (string Token, DateTime ExpiresAt) GenerateToken(User user, Guid tenantId, string sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("role", user.Role.ToString()),
            new Claim("session_id", sessionId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string Token, DateTime ExpiresAt) GenerateRefreshToken(Guid userId, Guid tenantId, string sessionId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddDays(7);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim("session_id", sessionId),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("is_refresh_token", "true"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public Application.Common.Interfaces.TokenValidationResult? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var handler = new JwtSecurityTokenHandler();

            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
                return null;

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            var sessionIdClaim = principal.FindFirst("session_id");
            var tenantIdClaim = principal.FindFirst("tenant_id");
            var emailClaim = principal.FindFirst(ClaimTypes.Email);
            var roleClaim = principal.FindFirst("role");

            if (userIdClaim is null || sessionIdClaim is null || tenantIdClaim is null)
                return null;

            return new Application.Common.Interfaces.TokenValidationResult
            {
                UserId = Guid.Parse(userIdClaim.Value),
                SessionId = Guid.Parse(sessionIdClaim.Value),
                TenantId = Guid.Parse(tenantIdClaim.Value),
                Email = emailClaim?.Value,
                Role = roleClaim?.Value ?? "Cliente",
                IssuedAt = jwtToken.IssuedAt,
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch
        {
            return null;
        }
    }

    public Guid? ExtractUserId(string token)
    {
        var result = ValidateToken(token);
        return result?.UserId;
    }

    public Guid? ExtractSessionId(string token)
    {
        var result = ValidateToken(token);
        return result?.SessionId;
    }
}
