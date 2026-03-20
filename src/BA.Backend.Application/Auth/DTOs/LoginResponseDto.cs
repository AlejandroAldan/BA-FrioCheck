namespace BA.Backend.Application.Auth.DTOs;

public record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    string UserFullName,
    string Role,
    Guid UserId,
    Guid TenantId,
    bool SessionReplaced,
    string RedirectTo
);
