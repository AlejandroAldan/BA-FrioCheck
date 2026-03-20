using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Infrastructure.Settings;
using Dapper;
using Microsoft.Data.SqlClient;

namespace BA.Backend.Infrastructure.Services;

public class SessionService : ISessionService
{
    private readonly DatabaseSettings _settings;

    public SessionService(DatabaseSettings settings)
    {
        _settings = settings;
    }

    public async Task RegisterSessionAsync(string sessionId, Guid userId, DateTime expiresAt, CancellationToken ct)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(ct);

        await connection.ExecuteAsync(
            @"INSERT INTO dbo.ActiveSessions (SessionId, UserId, IsRevoked, ExpiresAt, CreatedAt)
              VALUES (@SessionId, @UserId, 0, @ExpiresAt, GETUTCDATE())",
            new { SessionId = sessionId, UserId = userId, ExpiresAt = expiresAt },
            commandTimeout: 30
        );
    }

    public async Task RevokeSessionAsync(string sessionId, CancellationToken ct)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(ct);

        await connection.ExecuteAsync(
            @"UPDATE dbo.ActiveSessions
              SET IsRevoked = 1
              WHERE SessionId = @SessionId",
            new { SessionId = sessionId },
            commandTimeout: 30
        );
    }

    public async Task<bool> IsSessionValidAsync(string sessionId, CancellationToken ct)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(ct);

        var result = await connection.QuerySingleOrDefaultAsync<int>(
            @"SELECT COUNT(1)
              FROM dbo.ActiveSessions
              WHERE SessionId = @SessionId
                AND IsRevoked = 0
                AND ExpiresAt > GETUTCDATE()",
            new { SessionId = sessionId },
            commandTimeout: 30
        );

        return result > 0;
    }

    public async Task<bool> RevokeAllUserSessionsAsync(Guid userId, CancellationToken ct)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(ct);

        var rowsAffected = await connection.ExecuteAsync(
            @"UPDATE dbo.ActiveSessions
              SET IsRevoked = 1
              WHERE UserId = @UserId
                AND IsRevoked = 0",
            new { UserId = userId },
            commandTimeout: 30
        );

        return rowsAffected > 0;
    }

    public async Task<bool> IsLastSessionRevokedAsync(string sessionId, Guid userId, CancellationToken ct)
    {
        using var connection = new SqlConnection(_settings.ConnectionString);
        await connection.OpenAsync(ct);

        var result = await connection.QuerySingleOrDefaultAsync<int>(
            @"SELECT COUNT(1)
              FROM dbo.ActiveSessions
              WHERE UserId = @UserId
                AND SessionId = @SessionId
                AND IsRevoked = 1",
            new { UserId = userId, SessionId = sessionId },
            commandTimeout: 30
        );

        return result > 0;
    }
}
