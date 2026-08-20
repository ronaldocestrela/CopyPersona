namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public sealed record TelemetrySummaryDto
{
    public int TotalExecutions { get; init; }
    public int SuccessfulExecutions { get; init; }
    public int FailedExecutions { get; init; }
    public double SuccessRatePercent { get; init; }
    public long TotalPromptTokens { get; init; }
    public long TotalCompletionTokens { get; init; }
    public long TotalTokens => TotalPromptTokens + TotalCompletionTokens;
    public decimal TotalCostUSD { get; init; }
    public decimal TotalSubscriptionRevenueUSD { get; init; }
    public double LLMCostMarginPercent { get; init; }
    public double AverageLatencyMs { get; init; }

    public IReadOnlyList<DailyTokenUsageDto> DailyUsages { get; init; } = Array.Empty<DailyTokenUsageDto>();
    public IReadOnlyList<AgentUsageSummaryDto> AgentSummaries { get; init; } = Array.Empty<AgentUsageSummaryDto>();
    public IReadOnlyList<ModelUsageSummaryDto> ModelSummaries { get; init; } = Array.Empty<ModelUsageSummaryDto>();
}

public sealed record DailyTokenUsageDto
{
    public DateTime Date { get; init; }
    public long PromptTokens { get; init; }
    public long CompletionTokens { get; init; }
    public long TotalTokens => PromptTokens + CompletionTokens;
    public decimal CostUSD { get; init; }
    public int ExecutionCount { get; init; }
}

public sealed record AgentUsageSummaryDto
{
    public string AgentName { get; init; } = string.Empty;
    public int ExecutionCount { get; init; }
    public long TotalTokens { get; init; }
    public decimal CostUSD { get; init; }
    public double AverageLatencyMs { get; init; }
    public double SuccessRatePercent { get; init; }
}

public sealed record ModelUsageSummaryDto
{
    public string ModelUsed { get; init; } = string.Empty;
    public int ExecutionCount { get; init; }
    public long TotalTokens { get; init; }
    public decimal CostUSD { get; init; }
    public double ErrorRatePercent { get; init; }
}

public sealed record AgentExecutionLogDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string AgentName { get; init; } = string.Empty;
    public string ModelUsed { get; init; } = string.Empty;
    public string ProviderType { get; init; } = string.Empty;
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens { get; init; }
    public decimal EstimatedCostUSD { get; init; }
    public long LatencyMs { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public DateTime ExecutedAtUtc { get; init; }
}

public sealed record AnomalyAlertDto
{
    public string AlertType { get; init; } = string.Empty;
    public string Severity { get; init; } = "Warning"; // Warning, Critical
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid? TenantId { get; init; }
    public DateTime DetectedAtUtc { get; init; }
}
