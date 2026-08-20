namespace PersonaScript.Modules.Billing.Domain;

public interface IProcessedStripeEventRepository
{
    Task<ProcessedStripeEvent?> GetByIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task AddAsync(ProcessedStripeEvent processedEvent, CancellationToken cancellationToken = default);
}
