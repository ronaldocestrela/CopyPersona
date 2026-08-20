using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Impersonation;

public record StartImpersonationCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    string Reason,
    string? IpAddress = null) : ICommand<Guid>;

public sealed class StartImpersonationCommandHandler(
    IUserRepository userRepository,
    IAdminImpersonationLogRepository impersonationLogRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<StartImpersonationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(StartImpersonationCommand command, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var targetUser = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (targetUser is null)
        {
            return Result.Failure<Guid>(Error.NotFound("StartImpersonation.TargetUserNotFound", "Tenant/Usuário de destino não encontrado."));
        }

        var logResult = AdminImpersonationLog.Create(
            command.AdminUserId,
            command.AdminEmail,
            command.TargetTenantId,
            targetUser.Email,
            command.Reason,
            command.IpAddress);

        if (logResult.IsFailure)
        {
            return Result.Failure<Guid>(logResult.Error);
        }

        var log = logResult.Value;
        await impersonationLogRepository.AddAsync(log, cancellationToken);

        var auditDetails = JsonSerializer.Serialize(new { command.Reason, logId = log.Id });
        var audit = AdminAuditLog.Record(
            "START_IMPERSONATION",
            command.AdminUserId,
            command.AdminEmail,
            command.TargetTenantId,
            targetUser.Email,
            auditDetails).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success(log.Id);
    }
}

public record StopImpersonationCommand(
    Guid AdminUserId,
    string AdminEmail) : ICommand;

public sealed class StopImpersonationCommandHandler(
    IAdminImpersonationLogRepository impersonationLogRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<StopImpersonationCommand>
{
    public async Task<Result> Handle(StopImpersonationCommand command, CancellationToken cancellationToken)
    {
        var activeSession = await impersonationLogRepository.GetActiveSessionByAdminIdAsync(command.AdminUserId, cancellationToken);

        if (activeSession is null)
        {
            return Result.Success();
        }

        activeSession.StopSession();
        await impersonationLogRepository.UpdateAsync(activeSession, cancellationToken);

        var audit = AdminAuditLog.Record(
            "STOP_IMPERSONATION",
            command.AdminUserId,
            command.AdminEmail,
            activeSession.TargetTenantId,
            activeSession.TargetUserEmail,
            JsonSerializer.Serialize(new { sessionDurationMinutes = (DateTimeOffset.UtcNow - activeSession.StartedAt).TotalMinutes })).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
