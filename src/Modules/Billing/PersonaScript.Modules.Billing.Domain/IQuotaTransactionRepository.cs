namespace PersonaScript.Modules.Billing.Domain;

public interface IQuotaTransactionRepository
{
    Task AddAsync(QuotaTransaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuotaTransaction>> GetByTenantIdAsync(Guid tenantId, int limit = 50, CancellationToken cancellationToken = default);
}
