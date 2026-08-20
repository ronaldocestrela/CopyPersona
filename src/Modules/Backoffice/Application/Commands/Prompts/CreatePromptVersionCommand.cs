using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Prompts;

public record CreatePromptVersionCommand(
    string AgentName,
    string SystemPrompt,
    string UserPromptTemplate,
    string ParametersJson,
    string Description,
    string AdminEmail) : ICommand<Guid>;

public sealed class CreatePromptVersionCommandHandler : ICommandHandler<CreatePromptVersionCommand, Guid>
{
    private readonly IPromptTemplateRepository _promptRepository;
    private readonly IAdminAuditLogRepository _auditLogRepository;

    public CreatePromptVersionCommandHandler(
        IPromptTemplateRepository promptRepository,
        IAdminAuditLogRepository auditLogRepository)
    {
        _promptRepository = promptRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePromptVersionCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.AgentName))
        {
            return Result.Failure<Guid>(Error.Validation("CreatePromptVersion.AgentNameRequired", "O nome do agente é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(command.SystemPrompt))
        {
            return Result.Failure<Guid>(Error.Validation("CreatePromptVersion.SystemPromptRequired", "O System Prompt é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(command.UserPromptTemplate))
        {
            return Result.Failure<Guid>(Error.Validation("CreatePromptVersion.UserPromptTemplateRequired", "O User Prompt Template é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return Result.Failure<Guid>(Error.Validation("CreatePromptVersion.DescriptionRequired", "A descrição do changelog é obrigatória."));
        }

        var latestVersionNumber = await _promptRepository.GetLatestVersionNumberAsync(command.AgentName, cancellationToken);
        var newVersionNumber = latestVersionNumber + 1;

        // Desativa a versão ativa anterior
        var currentActive = await _promptRepository.GetActiveByAgentNameAsync(command.AgentName, cancellationToken);
        if (currentActive != null)
        {
            currentActive.Deactivate();
            await _promptRepository.UpdateAsync(currentActive, cancellationToken);
        }

        // Cria a nova versão e ativa
        var createResult = PromptTemplate.Create(
            command.AgentName,
            newVersionNumber,
            command.SystemPrompt,
            command.UserPromptTemplate,
            command.ParametersJson,
            command.Description,
            command.AdminEmail,
            isActive: true);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var newPrompt = createResult.Value;
        await _promptRepository.AddAsync(newPrompt, cancellationToken);

        // Audit Log
        var details = JsonSerializer.Serialize(new
        {
            newPrompt.AgentName,
            newPrompt.Version,
            newPrompt.Description,
            PreviousVersion = currentActive?.Version
        });

        var auditLogResult = AdminAuditLog.Record(
            actionType: "CREATE_PROMPT_VERSION",
            adminUserId: Guid.Empty,
            adminEmail: command.AdminEmail,
            targetTenantId: Guid.Empty,
            targetUserEmail: "system",
            detailsJson: details);

        if (auditLogResult.IsSuccess)
        {
            await _auditLogRepository.AddAsync(auditLogResult.Value, cancellationToken);
        }

        return Result.Success(newPrompt.Id);
    }
}
