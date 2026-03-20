using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        // The token is stored only in memory during the request,
        // we query by the hash which is stored in the database
        return await _context.PasswordResetTokens
        .FirstOrDefaultAsync(t => t.TokenHash == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        PasswordResetToken token,
        CancellationToken cancellationToken = default)
    {
        _context.PasswordResetTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var token = await _context.PasswordResetTokens.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (token != null)
        {
            _context.PasswordResetTokens.Remove(token);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<PasswordResetToken>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PasswordResetTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
