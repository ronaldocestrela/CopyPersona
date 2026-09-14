using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Server.Observability.Metrics;

namespace PersonaScript.Server.Observability.Alerts;

public sealed class OperationalLLMFailureNotifier : ILLMFailureNotifier
{
    private readonly IOperationalAlertService _alertService;

    public OperationalLLMFailureNotifier(IOperationalAlertService alertService)
    {
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
    }

    public async Task NotifyFailureAsync(
        LLMProviderType providerType,
        string? errorCode,
        string errorMessage,
        bool allProvidersFailed,
        CancellationToken cancellationToken = default)
    {
        PersonaScriptMetrics.RecordLLMFailure(providerType.ToString(), errorCode ?? "Unknown");

        var alert = new LLMFailureAlert(
            ProviderType: providerType.ToString(),
            Model: "default",
            ErrorCode: errorCode,
            ErrorMessage: errorMessage,
            LatencyMs: 0,
            AllProvidersFailed: allProvidersFailed,
            TimestampUtc: DateTime.UtcNow);

        await _alertService.NotifyLLMQuotaOrFailureAsync(alert, cancellationToken);
    }
}
