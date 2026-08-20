using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.GrantExtraCredits;

public record GrantTenantExtraCreditsCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    int ExtraScripts,
    int ExtraAiAnalyses,
    string Reason) : ICommand;

public sealed class GrantTenantExtraCreditsCommandHandler(
    IUserRepository userRepository,
    IUsageQuotaRepository usageQuotaRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<GrantTenantExtraCreditsCommand>
{
    public async Task<Result> Handle(GrantTenantExtraCreditsCommand command, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("GrantExtraCredits.UserNotFound", "Tenant/Usuário não encontrado."));
        }

        var quota = await usageQuotaRepository.GetByTenantIdAsync(command.TargetTenantId, cancellationToken);
        if (quota is null)
        {
            return Result.Failure(Error.NotFound("GrantExtraCredits.QuotaNotFound", "Quota de uso do tenant não encontrada."));
        }

        var grantResult = quota.GrantExtraCredits(command.ExtraScripts, command.ExtraAiAnalyses, command.Reason);
        if (grantResult.IsFailure)
        {
            return grantResult;
        }

        await usageQuotaRepository.UpdateAsync(quota, cancellationToken);

        var audit = AdminAuditLog.Record(
            "GRANT_EXTRA_CREDITS",
            command.AdminUserId,
            command.AdminEmail,
            command.TargetTenantId,
            user.Email,
            JsonSerializer.Serialize(new { command.ExtraScripts, command.ExtraAiAnalyses, command.Reason })).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
