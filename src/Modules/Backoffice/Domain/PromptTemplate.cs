using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class PromptTemplate : BaseEntity
{
    public string AgentName { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public string SystemPrompt { get; private set; } = string.Empty;
    public string UserPromptTemplate { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string ParametersJson { get; private set; } = "{}";
    public string Description { get; private set; } = string.Empty;
    public string CreatedByAdminEmail { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }

    private PromptTemplate() { }

    public static Result<PromptTemplate> Create(
        string agentName,
        int version,
        string systemPrompt,
        string userPromptTemplate,
        string parametersJson,
        string description,
        string adminEmail,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(agentName))
        {
            return Result.Failure<PromptTemplate>(Error.Validation("PromptTemplate.AgentNameRequired", "O nome do agente é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            return Result.Failure<PromptTemplate>(Error.Validation("PromptTemplate.SystemPromptRequired", "O System Prompt é obrigatório."));
        }

        if (string.IsNullOrWhiteSpace(userPromptTemplate))
        {
            return Result.Failure<PromptTemplate>(Error.Validation("PromptTemplate.UserPromptTemplateRequired", "O User Prompt Template é obrigatório."));
        }

        if (version <= 0)
        {
            return Result.Failure<PromptTemplate>(Error.Validation("PromptTemplate.InvalidVersion", "A versão deve ser maior que zero."));
        }

        var template = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            AgentName = agentName.Trim(),
            Version = version,
            SystemPrompt = systemPrompt.Trim(),
            UserPromptTemplate = userPromptTemplate.Trim(),
            ParametersJson = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? "Sem descrição" : description.Trim(),
            CreatedByAdminEmail = string.IsNullOrWhiteSpace(adminEmail) ? "sistema" : adminEmail.Trim().ToLowerInvariant(),
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(template);
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
