using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.Modules.Billing.Domain;


public class QuotaTransaction : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid QuotaId { get; private set; }
    public QuotaResourceType ResourceType { get; private set; }
    public int Quantity { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? SourceCommand { get; private set; }

    private QuotaTransaction() { }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static QuotaTransaction Record(
        Guid tenantId,
        Guid quotaId,
        QuotaResourceType resourceType,
        int quantity,
        string description,
        string? sourceCommand = null)
    {
        return new QuotaTransaction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            QuotaId = quotaId,
            ResourceType = resourceType,
            Quantity = quantity,
            TransactionDate = DateTime.UtcNow,
            Description = description,
            SourceCommand = sourceCommand
        };
    }
}
