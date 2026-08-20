using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class AgentExecutionLog : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string AgentName { get; private set; } = string.Empty;
    public string ModelUsed { get; private set; } = string.Empty;
    public string ProviderType { get; private set; } = string.Empty;
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public int TotalTokens => PromptTokens + CompletionTokens;
    public decimal EstimatedCostUSD { get; private set; }
    public long LatencyMs { get; private set; }
    public AgentExecutionStatus Status { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime ExecutedAtUtc { get; private set; }

    private AgentExecutionLog() { }

    public static Result<AgentExecutionLog> Create(
        Guid tenantId,
        string agentName,
        string modelUsed,
        string providerType,
        int promptTokens,
        int completionTokens,
        decimal estimatedCostUsd,
        long latencyMs,
        AgentExecutionStatus status,
        string? errorMessage = null)
    {
        if (string.IsNullOrWhiteSpace(agentName))
        {
            return Result.Failure<AgentExecutionLog>(Error.Validation("AgentExecutionLog.AgentNameRequired", "O nome do agente é obrigatório."));
        }

        var log = new AgentExecutionLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AgentName = agentName.Trim(),
            ModelUsed = string.IsNullOrWhiteSpace(modelUsed) ? "unknown" : modelUsed.Trim(),
            ProviderType = string.IsNullOrWhiteSpace(providerType) ? "Unknown" : providerType.Trim(),
            PromptTokens = Math.Max(0, promptTokens),
            CompletionTokens = Math.Max(0, completionTokens),
            EstimatedCostUSD = Math.Max(0m, estimatedCostUsd),
            LatencyMs = Math.Max(0, latencyMs),
            Status = status,
            ErrorMessage = errorMessage,
            ExecutedAtUtc = DateTime.UtcNow
        };

        return Result.Success(log);
    }
}
