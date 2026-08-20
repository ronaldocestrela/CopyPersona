using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class AdminAuditLogRepository(BackofficeDbContext dbContext) : IAdminAuditLogRepository
{
    public async Task AddAsync(AdminAuditLog log, CancellationToken cancellationToken = default)
    {
        await dbContext.AuditLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminAuditLog>> GetLogsByTargetTenantIdAsync(Guid targetTenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .Where(x => x.TargetTenantId == targetTenantId)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminAuditLog>> GetAllAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await dbContext.AuditLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
