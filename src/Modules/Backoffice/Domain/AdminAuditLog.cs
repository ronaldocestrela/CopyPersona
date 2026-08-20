using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class AdminAuditLog : BaseEntity
{
    public string ActionType { get; private set; } = string.Empty;
    public Guid AdminUserId { get; private set; }
    public string AdminEmail { get; private set; } = string.Empty;
    public Guid TargetTenantId { get; private set; }
    public string TargetUserEmail { get; private set; } = string.Empty;
    public string DetailsJson { get; private set; } = "{}";
    public DateTimeOffset Timestamp { get; private set; }

    private AdminAuditLog() { }

    public static Result<AdminAuditLog> Record(
        string actionType,
        Guid adminUserId,
        string adminEmail,
        Guid targetTenantId,
        string targetUserEmail,
        string detailsJson)
    {
        if (string.IsNullOrWhiteSpace(actionType))
        {
            return Result.Failure<AdminAuditLog>(Error.Validation("AdminAuditLog.ActionTypeRequired", "O tipo de ação é obrigatório."));
        }

        var log = new AdminAuditLog
        {
            Id = Guid.NewGuid(),
            ActionType = actionType.Trim().ToUpperInvariant(),
            AdminUserId = adminUserId,
            AdminEmail = adminEmail.Trim().ToLowerInvariant(),
            TargetTenantId = targetTenantId,
            TargetUserEmail = targetUserEmail.Trim().ToLowerInvariant(),
            DetailsJson = string.IsNullOrWhiteSpace(detailsJson) ? "{}" : detailsJson,
            Timestamp = DateTimeOffset.UtcNow
        };

        return Result.Success(log);
    }
}
