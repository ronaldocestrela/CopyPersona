using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PersonaScript.Modules.Backoffice.Application.Abstractions;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public sealed class LLMTelemetryService : ILLMTelemetryService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILLMCostCalculator _costCalculator;
    private readonly ILogger<LLMTelemetryService> _logger;

    public LLMTelemetryService(
        IServiceScopeFactory scopeFactory,
        ILLMCostCalculator costCalculator,
        ILogger<LLMTelemetryService> logger)
    {
        _scopeFactory = scopeFactory;
        _costCalculator = costCalculator;
        _logger = logger;
    }

    public async Task RecordExecutionAsync(
        Guid tenantId,
        string agentName,
        string modelUsed,
        string providerType,
        int promptTokens,
        int completionTokens,
        long latencyMs,
        AgentExecutionStatus status,
        string? errorMessage = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var estimatedCost = _costCalculator.CalculateCost(modelUsed, promptTokens, completionTokens);

            var logResult = AgentExecutionLog.Create(
                tenantId,
                agentName,
                modelUsed,
                providerType,
                promptTokens,
                completionTokens,
                estimatedCost,
                latencyMs,
                status,
                errorMessage);

            if (logResult.IsFailure)
            {
                _logger.LogWarning("Falha ao criar log de telemetria LLM: {Error}", logResult.Error.Message);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IAgentExecutionLogRepository>();
            await repository.AddAsync(logResult.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao gravar telemetria de execução de LLM para agente {AgentName}", agentName);
        }
    }
}
