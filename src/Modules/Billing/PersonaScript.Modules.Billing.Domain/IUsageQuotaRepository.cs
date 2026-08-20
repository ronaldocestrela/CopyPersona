namespace PersonaScript.Modules.Billing.Domain;

public interface IUsageQuotaRepository
{
    Task<UsageQuota?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<UsageQuota?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsageQuota>> GetExpiredQuotasAsync(DateTime beforeDate, CancellationToken cancellationToken = default);
    Task AddAsync(UsageQuota quota, CancellationToken cancellationToken = default);
    void Update(UsageQuota quota);
    Task UpdateAsync(UsageQuota quota, CancellationToken cancellationToken = default);
}

