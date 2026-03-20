using BA.Backend.Domain.Entities;

namespace BA.Backend.Application.Common.Interfaces;

public interface ISessionService
{
    Task RegisterSessionAsync(string sessionId, Guid userId, DateTime expiresAt, CancellationToken ct);
    Task RevokeSessionAsync(string sessionId, CancellationToken ct);
    Task<bool> IsSessionValidAsync(string sessionId, CancellationToken ct);
    Task<bool> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct);
    Task<bool> IsLastSessionRevokedAsync(string sessionId, Guid userId, CancellationToken ct);
}
