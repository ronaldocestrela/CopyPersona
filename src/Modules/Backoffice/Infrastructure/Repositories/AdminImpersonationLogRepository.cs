using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class AdminImpersonationLogRepository(BackofficeDbContext dbContext) : IAdminImpersonationLogRepository
{
    public async Task AddAsync(AdminImpersonationLog log, CancellationToken cancellationToken = default)
    {
        await dbContext.ImpersonationLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AdminImpersonationLog?> GetActiveSessionByAdminIdAsync(Guid adminUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImpersonationLogs
            .Where(x => x.AdminUserId == adminUserId && x.EndedAt == null)
            .OrderByDescending(x => x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminImpersonationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImpersonationLogs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminImpersonationLog>> GetLogsByTargetTenantIdAsync(Guid targetTenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImpersonationLogs
            .Where(x => x.TargetTenantId == targetTenantId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminImpersonationLog>> GetAllLogsAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await dbContext.ImpersonationLogs
            .OrderByDescending(x => x.StartedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(AdminImpersonationLog log, CancellationToken cancellationToken = default)
    {
        dbContext.ImpersonationLogs.Update(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
