using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;

namespace PersonaScript.Modules.Billing.Infrastructure.Repositories;

public sealed class UsageQuotaRepository(BillingDbContext dbContext) : IUsageQuotaRepository
{
    public async Task<UsageQuota?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UsageQuotas.FirstOrDefaultAsync(q => q.TenantId == tenantId, cancellationToken);
    }

    public async Task<UsageQuota?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UsageQuotas.FirstOrDefaultAsync(q => q.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task<IReadOnlyList<UsageQuota>> GetExpiredQuotasAsync(DateTime beforeDate, CancellationToken cancellationToken = default)
    {
        return await dbContext.UsageQuotas
            .IgnoreQueryFilters()
            .Where(q => q.PeriodEnd <= beforeDate)
            .ToListAsync(cancellationToken);
    }


    public async Task AddAsync(UsageQuota quota, CancellationToken cancellationToken = default)
    {
        await dbContext.UsageQuotas.AddAsync(quota, cancellationToken);
    }

    public void Update(UsageQuota quota)
    {
        dbContext.UsageQuotas.Update(quota);
    }

    public async Task UpdateAsync(UsageQuota quota, CancellationToken cancellationToken = default)
    {
        dbContext.UsageQuotas.Update(quota);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
