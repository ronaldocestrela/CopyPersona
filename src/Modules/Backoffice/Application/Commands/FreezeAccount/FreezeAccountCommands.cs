using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.FreezeAccount;

public record FreezeTenantAccountCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    string Reason) : ICommand;

public sealed class FreezeTenantAccountCommandHandler(
    IUserRepository userRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<FreezeTenantAccountCommand>
{
    public async Task<Result> Handle(FreezeTenantAccountCommand command, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("FreezeTenant.UserNotFound", "Usuário/Tenant não encontrado."));
        }

        var freezeResult = user.Freeze(command.Reason);
        if (freezeResult.IsFailure)
        {
            return freezeResult;
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        var audit = AdminAuditLog.Record(
            "FREEZE_ACCOUNT",
            command.AdminUserId,
            command.AdminEmail,
            user.TenantId,
            user.Email,
            JsonSerializer.Serialize(new { command.Reason, frozenAt = user.FrozenAt })).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}

public record UnfreezeTenantAccountCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId) : ICommand;

public sealed class UnfreezeTenantAccountCommandHandler(
    IUserRepository userRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<UnfreezeTenantAccountCommand>
{
    public async Task<Result> Handle(UnfreezeTenantAccountCommand command, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("UnfreezeTenant.UserNotFound", "Usuário/Tenant não encontrado."));
        }

        user.Unfreeze();
        await userRepository.UpdateAsync(user, cancellationToken);

        var audit = AdminAuditLog.Record(
            "UNFREEZE_ACCOUNT",
            command.AdminUserId,
            command.AdminEmail,
            user.TenantId,
            user.Email,
            "{}").Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
