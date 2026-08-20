using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;

namespace PersonaScript.Modules.Identity.Infrastructure.Repositories;

public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.TenantId == tenantId, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
