namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public record PromptTemplateDto(
    Guid Id,
    string AgentName,
    int Version,
    string SystemPrompt,
    string UserPromptTemplate,
    bool IsActive,
    string ParametersJson,
    string Description,
    string CreatedByAdminEmail,
    DateTimeOffset CreatedAt);

public record TestPromptRequestDto(
    string AgentName,
    string SystemPrompt,
    string UserPromptTemplate,
    string ParametersJson,
    string TestVariablesJson);

public record TestPromptResultDto(
    bool Success,
    string ResponseContent,
    long LatencyMs,
    int PromptTokens,
    int CompletionTokens,
    string? ErrorMessage = null);
