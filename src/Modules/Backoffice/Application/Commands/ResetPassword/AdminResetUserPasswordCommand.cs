using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.ResetPassword;

public record AdminResetUserPasswordCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid TargetTenantId,
    string NewPassword) : ICommand;

public sealed class AdminResetUserPasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<AdminResetUserPasswordCommand>
{
    public async Task<Result> Handle(AdminResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
        {
            return Result.Failure(Error.Validation("AdminResetPassword.TooShort", "A nova senha deve ter no mínimo 8 caracteres."));
        }

        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == command.TargetTenantId);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("AdminResetPassword.UserNotFound", "Usuário não encontrado."));
        }

        var newHash = passwordHasher.HashPassword(command.NewPassword);
        user.SetAdminPasswordHash(newHash);

        await userRepository.UpdateAsync(user, cancellationToken);

        var audit = AdminAuditLog.Record(
            "RESET_USER_PASSWORD",
            command.AdminUserId,
            command.AdminEmail,
            user.TenantId,
            user.Email,
            JsonSerializer.Serialize(new { action = "Password reset by admin" })).Value;

        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
