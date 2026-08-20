using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Billing.Domain;

public class ProcessedStripeEvent
{
    public string Id { get; private set; } = string.Empty; // Stripe Event ID (e.g. evt_123456)
    public string EventType { get; private set; } = string.Empty;
    public Guid? TenantId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private ProcessedStripeEvent() { }

    public static Result<ProcessedStripeEvent> Create(string eventId, string eventType, Guid? tenantId = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return Result.Failure<ProcessedStripeEvent>(Error.Validation("ProcessedStripeEvent.EventIdRequired", "O Id do evento do Stripe é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            return Result.Failure<ProcessedStripeEvent>(Error.Validation("ProcessedStripeEvent.EventTypeRequired", "O tipo do evento do Stripe é obrigatório."));
        }

        var item = new ProcessedStripeEvent
        {
            Id = eventId.Trim(),
            EventType = eventType.Trim(),
            TenantId = tenantId,
            ProcessedAt = DateTime.UtcNow
        };

        return Result.Success(item);
    }
}
