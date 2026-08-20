using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Application.Abstractions;

public interface ILLMTelemetryService
{
    Task RecordExecutionAsync(
        Guid tenantId,
        string agentName,
        string modelUsed,
        string providerType,
        int promptTokens,
        int completionTokens,
        long latencyMs,
        AgentExecutionStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default);
}
