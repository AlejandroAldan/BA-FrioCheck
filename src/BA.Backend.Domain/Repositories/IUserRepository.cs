using BA.Backend.Domain.Entities;
namespace BA.Backend.Domain.Repositories;

public interface IUserRepository
{
    // metodos para leer (Query)
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct);

    //metodos para escribir (Command)
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}