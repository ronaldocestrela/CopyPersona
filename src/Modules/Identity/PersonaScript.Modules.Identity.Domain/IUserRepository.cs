namespace PersonaScript.Modules.Identity.Domain;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
