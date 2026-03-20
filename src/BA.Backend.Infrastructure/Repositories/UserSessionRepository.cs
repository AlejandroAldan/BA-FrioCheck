using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

/// <summary>
/// Implementación del UserSessionRepository
/// Gestiona las sesiones de usuario e implementa la lógica de sesión única por dispositivo
/// </summary>
public class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _context;

    public UserSessionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession?> GetActiveSessionByDeviceAsync(Guid userId, string deviceId)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => 
                s.UserId == userId && 
                s.DeviceId == deviceId && 
                s.IsActive);
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsByUserAsync(Guid userId)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
    }

    public async Task<UserSession?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<UserSession> CreateSessionAsync(UserSession session)
    {
        session.CreatedAt = DateTime.UtcNow;

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<bool> InvalidateSessionAsync(Guid sessionId, string reason)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null)
            return false;

        session.IsActive = false;
        session.ClosedAt = DateTime.UtcNow;
        session.ClosureReason = reason;

        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> InvalidateAllUserSessionsAsync(Guid userId, string reason)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IsActive = false;
            session.ClosedAt = DateTime.UtcNow;
            session.ClosureReason = reason;
        }

        if (sessions.Count > 0)
        {
            _context.UserSessions.UpdateRange(sessions);
            await _context.SaveChangesAsync();
        }

        return sessions.Count;
    }

    public async Task<int> InvalidatePreviousSessionAsync(Guid userId, string newDeviceId)
    {
        var previousSessions = await _context.UserSessions
            .Where(s => 
                s.UserId == userId && 
                s.DeviceId != newDeviceId && 
                s.IsActive)
            .ToListAsync();

        foreach (var session in previousSessions)
        {
            session.IsActive = false;
            session.ClosedAt = DateTime.UtcNow;
            session.ClosureReason = "Nueva sesión iniciada en otro dispositivo";
        }

        if (previousSessions.Count > 0)
        {
            _context.UserSessions.UpdateRange(previousSessions);
            await _context.SaveChangesAsync();
        }

        return previousSessions.Count;
    }

    public async Task<bool> UpdateLastActivityAsync(Guid sessionId)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
        if (session is null)
            return false;

        session.LastActivityAt = DateTime.UtcNow;
        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UserSession?> GetSessionByTokenAsync(string token)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.JwtToken == token && s.IsActive);
    }
}
