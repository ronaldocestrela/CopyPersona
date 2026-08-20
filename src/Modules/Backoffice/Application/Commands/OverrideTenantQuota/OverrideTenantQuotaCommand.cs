using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota;

public record OverrideTenantQuotaCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    int ScriptsLimit,
    int PersonasLimit,
    int AiAnalysesLimit,
    string Reason) : ICommand;

public sealed class OverrideTenantQuotaCommandHandler(
    IUserRepository userRepository,
    IUsageQuotaRepository usageQuotaRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<OverrideTenantQuotaCommand>
{
    public async Task<Result> Handle(OverrideTenantQuotaCommand command, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("OverrideTenantQuota.UserNotFound", "Tenant/Usuário não encontrado."));
        }

        var quota = await usageQuotaRepository.GetByTenantIdAsync(command.TargetTenantId, cancellationToken);
        if (quota is null)
        {
            return Result.Failure(Error.NotFound("OverrideTenantQuota.QuotaNotFound", "Quota de uso do tenant não encontrada."));
        }

        var overrideResult = quota.OverrideLimits(
            command.ScriptsLimit,
            command.PersonasLimit,
            command.AiAnalysesLimit,
            command.Reason);

        if (overrideResult.IsFailure)
        {
            return overrideResult;
        }

        await usageQuotaRepository.UpdateAsync(quota, cancellationToken);

        var audit = AdminAuditLog.Record(
            "OVERRIDE_TENANT_QUOTA",
            command.AdminUserId,
            command.AdminEmail,
            command.TargetTenantId,
            user.Email,
            JsonSerializer.Serialize(new
            {
                command.ScriptsLimit,
                command.PersonasLimit,
                command.AiAnalysesLimit,
                command.Reason
            })).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
