using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BA.Backend.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, ct);
    }

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
    }
}
