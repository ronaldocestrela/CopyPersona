using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.Tenancy;

public interface IImpersonationService
{
    bool IsImpersonating { get; }
    Guid? TargetTenantId { get; }
    string? TargetUserEmail { get; }
    Task<Result> StartImpersonationAsync(Guid adminUserId, string adminEmail, Guid targetTenantId, string targetUserEmail, string reason, CancellationToken cancellationToken = default);
    Task<Result> StopImpersonationAsync(CancellationToken cancellationToken = default);
}
