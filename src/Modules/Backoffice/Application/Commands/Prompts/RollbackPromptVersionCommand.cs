using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Prompts;

public record RollbackPromptVersionCommand(
    Guid TargetPromptId,
    string Reason,
    string AdminEmail) : ICommand;

public sealed class RollbackPromptVersionCommandHandler : ICommandHandler<RollbackPromptVersionCommand>
{
    private readonly IPromptTemplateRepository _promptRepository;
    private readonly IAdminAuditLogRepository _auditLogRepository;

    public RollbackPromptVersionCommandHandler(
        IPromptTemplateRepository promptRepository,
        IAdminAuditLogRepository auditLogRepository)
    {
        _promptRepository = promptRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result> Handle(RollbackPromptVersionCommand command, CancellationToken cancellationToken)
    {
        if (command.TargetPromptId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("RollbackPromptVersion.TargetIdRequired", "O ID do prompt alvo é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Result.Failure(Error.Validation("RollbackPromptVersion.ReasonRequired", "O motivo do rollback é obrigatório."));
        }

        var targetPrompt = await _promptRepository.GetByIdAsync(command.TargetPromptId, cancellationToken);
        if (targetPrompt == null)
        {
            return Result.Failure(Error.NotFound("RollbackPromptVersion.NotFound", "A versão de prompt solicitada não foi encontrada."));
        }

        if (targetPrompt.IsActive)
        {
            return Result.Failure(Error.Validation("RollbackPromptVersion.AlreadyActive", "A versão selecionada já é a versão ativa."));
        }

        // Desativa a versão atualmente ativa do agente
        var currentActive = await _promptRepository.GetActiveByAgentNameAsync(targetPrompt.AgentName, cancellationToken);
        if (currentActive != null)
        {
            currentActive.Deactivate();
            await _promptRepository.UpdateAsync(currentActive, cancellationToken);
        }

        // Ativa o prompt alvo
        targetPrompt.Activate();
        await _promptRepository.UpdateAsync(targetPrompt, cancellationToken);

        // Registra log de auditoria
        var details = JsonSerializer.Serialize(new
        {
            targetPrompt.AgentName,
            RestoredVersion = targetPrompt.Version,
            DeactivatedVersion = currentActive?.Version,
            command.Reason
        });

        var auditLogResult = AdminAuditLog.Record(
            actionType: "ROLLBACK_PROMPT_VERSION",
            adminUserId: Guid.Empty,
            adminEmail: command.AdminEmail,
            targetTenantId: Guid.Empty,
            targetUserEmail: "system",
            detailsJson: details);

        if (auditLogResult.IsSuccess)
        {
            await _auditLogRepository.AddAsync(auditLogResult.Value, cancellationToken);
        }

        return Result.Success();
    }
}
