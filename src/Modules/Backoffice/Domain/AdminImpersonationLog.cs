using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class AdminImpersonationLog : BaseEntity
{
    public Guid AdminUserId { get; private set; }
    public string AdminEmail { get; private set; } = string.Empty;
    public Guid TargetTenantId { get; private set; }
    public string TargetUserEmail { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public string? IpAddress { get; private set; }

    private AdminImpersonationLog() { }

    public static Result<AdminImpersonationLog> Create(
        Guid adminUserId,
        string adminEmail,
        Guid targetTenantId,
        string targetUserEmail,
        string reason,
        string? ipAddress = null)
    {
        if (adminUserId == Guid.Empty)
        {
            return Result.Failure<AdminImpersonationLog>(Error.Validation("AdminImpersonationLog.AdminUserIdRequired", "O Id do administrador é obrigatório."));
        }

        if (targetTenantId == Guid.Empty)
        {
            return Result.Failure<AdminImpersonationLog>(Error.Validation("AdminImpersonationLog.TargetTenantIdRequired", "O TenantId de destino é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10)
        {
            return Result.Failure<AdminImpersonationLog>(Error.Validation("AdminImpersonationLog.ReasonRequired", "O motivo da impersonação é obrigatório e deve ter no mínimo 10 caracteres."));
        }

        var log = new AdminImpersonationLog
        {
            Id = Guid.NewGuid(),
            AdminUserId = adminUserId,
            AdminEmail = adminEmail.Trim().ToLowerInvariant(),
            TargetTenantId = targetTenantId,
            TargetUserEmail = targetUserEmail.Trim().ToLowerInvariant(),
            Reason = reason.Trim(),
            StartedAt = DateTimeOffset.UtcNow,
            IpAddress = ipAddress
        };

        return Result.Success(log);
    }

    public Result StopSession()
    {
        if (EndedAt.HasValue)
        {
            return Result.Success();
        }

        EndedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}
