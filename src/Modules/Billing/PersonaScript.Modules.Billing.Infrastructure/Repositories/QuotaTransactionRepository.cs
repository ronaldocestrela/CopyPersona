using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;

namespace PersonaScript.Modules.Billing.Infrastructure.Repositories;

public sealed class QuotaTransactionRepository(BillingDbContext dbContext) : IQuotaTransactionRepository
{
    public async Task AddAsync(QuotaTransaction transaction, CancellationToken cancellationToken = default)
    {
        await dbContext.QuotaTransactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<IReadOnlyList<QuotaTransaction>> GetByTenantIdAsync(Guid tenantId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await dbContext.QuotaTransactions
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
