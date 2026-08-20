namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface IAdminImpersonationLogRepository
{
    Task AddAsync(AdminImpersonationLog log, CancellationToken cancellationToken = default);
    Task<AdminImpersonationLog?> GetActiveSessionByAdminIdAsync(Guid adminUserId, CancellationToken cancellationToken = default);
    Task<AdminImpersonationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminImpersonationLog>> GetLogsByTargetTenantIdAsync(Guid targetTenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminImpersonationLog>> GetAllLogsAsync(int take = 100, CancellationToken cancellationToken = default);
    Task UpdateAsync(AdminImpersonationLog log, CancellationToken cancellationToken = default);
}
