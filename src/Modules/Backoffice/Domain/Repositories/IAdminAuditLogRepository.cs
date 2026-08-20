namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface IAdminAuditLogRepository
{
    Task AddAsync(AdminAuditLog log, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminAuditLog>> GetLogsByTargetTenantIdAsync(Guid targetTenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminAuditLog>> GetAllAsync(int take = 100, CancellationToken cancellationToken = default);
}
