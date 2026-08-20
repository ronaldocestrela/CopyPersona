namespace PersonaScript.Modules.Billing.Domain;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
    Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default);
    void Update(Subscription subscription);
}
