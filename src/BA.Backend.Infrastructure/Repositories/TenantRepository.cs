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

    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive, ct);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct)
    {
        return await _context.Tenants
            .FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive, ct);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct)
    {
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tenant tenant, CancellationToken ct)
    {
        _context.Tenants.Update(tenant);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var tenant = await _context.Tenants.FindAsync(new object[] { id }, cancellationToken: ct);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync(ct);
        }
    }
}
