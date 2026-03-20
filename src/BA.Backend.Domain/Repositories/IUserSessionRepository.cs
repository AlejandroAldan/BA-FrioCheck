using BA.Backend.Domain.Entities;

namespace BA.Backend.Domain.Repositories;

public interface IUserSessionRepository
{
    // metodos para leer (Query)
    Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceId);
    Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId);
    Task<UserSession?> GetSessionByIdAsync(Guid sessionId);
    Task<UserSession?> GetSessionByTokenAsync(string token);

    //metodos para escribir (Command)
    Task<UserSession> CreateSessionAsync(UserSession session);
    Task<bool> InvalidateSessionAsync(Guid sessionId, string reason);
    Task<int> InvalidateAllUserSessionsAsync(Guid userId, string reason);
    Task<int> InvalidatePreviousSessionAsync(Guid userId, string newDeviceId);
    Task<bool> UpdateLastActivityAsync(Guid sessionId);
}