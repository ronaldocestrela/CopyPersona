using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Telemetry;

public sealed record GetTelemetrySummaryQuery(DateTime StartDate, DateTime EndDate) : IQuery<TelemetrySummaryDto>;

public sealed class GetTelemetrySummaryQueryHandler : IQueryHandler<GetTelemetrySummaryQuery, TelemetrySummaryDto>
{
    private readonly IAgentExecutionLogRepository _repository;

    public GetTelemetrySummaryQueryHandler(IAgentExecutionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<TelemetrySummaryDto>> Handle(GetTelemetrySummaryQuery query, CancellationToken cancellationToken)
    {
        var logs = await _repository.GetLogsInPeriodAsync(query.StartDate, query.EndDate, cancellationToken);

        if (logs.Count == 0)
        {
            return Result.Success(new TelemetrySummaryDto());
        }

        var totalExecutions = logs.Count;
        var successful = logs.Count(x => x.Status == AgentExecutionStatus.Success);
        var failed = logs.Count(x => x.Status == AgentExecutionStatus.Failure);
        var successRate = totalExecutions > 0 ? (double)successful / totalExecutions * 100 : 0;

        var totalPrompt = logs.Sum(x => (long)x.PromptTokens);
        var totalCompletion = logs.Sum(x => (long)x.CompletionTokens);
        var totalCost = logs.Sum(x => x.EstimatedCostUSD);
        var avgLatency = logs.Average(x => (double)x.LatencyMs);

        // Group by Date for Daily Usage
        var dailyUsages = logs
            .GroupBy(x => x.ExecutedAtUtc.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyTokenUsageDto
            {
                Date = g.Key,
                PromptTokens = g.Sum(x => (long)x.PromptTokens),
                CompletionTokens = g.Sum(x => (long)x.CompletionTokens),
                CostUSD = g.Sum(x => x.EstimatedCostUSD),
                ExecutionCount = g.Count()
            })
            .ToList();

        // Group by Agent
        var agentSummaries = logs
            .GroupBy(x => x.AgentName)
            .Select(g => new AgentUsageSummaryDto
            {
                AgentName = g.Key,
                ExecutionCount = g.Count(),
                TotalTokens = g.Sum(x => (long)x.TotalTokens),
                CostUSD = g.Sum(x => x.EstimatedCostUSD),
                AverageLatencyMs = g.Average(x => (double)x.LatencyMs),
                SuccessRatePercent = (double)g.Count(x => x.Status == AgentExecutionStatus.Success) / g.Count() * 100
            })
            .OrderByDescending(x => x.CostUSD)
            .ToList();

        // Group by Model
        var modelSummaries = logs
            .GroupBy(x => x.ModelUsed)
            .Select(g => new ModelUsageSummaryDto
            {
                ModelUsed = g.Key,
                ExecutionCount = g.Count(),
                TotalTokens = g.Sum(x => (long)x.TotalTokens),
                CostUSD = g.Sum(x => x.EstimatedCostUSD),
                ErrorRatePercent = (double)g.Count(x => x.Status == AgentExecutionStatus.Failure) / g.Count() * 100
            })
            .OrderByDescending(x => x.ExecutionCount)
            .ToList();

        var summary = new TelemetrySummaryDto
        {
            TotalExecutions = totalExecutions,
            SuccessfulExecutions = successful,
            FailedExecutions = failed,
            SuccessRatePercent = Math.Round(successRate, 1),
            TotalPromptTokens = totalPrompt,
            TotalCompletionTokens = totalCompletion,
            TotalCostUSD = Math.Round(totalCost, 4),
            TotalSubscriptionRevenueUSD = 2490.00m, // Referência de Receita Mensal Recorrente estimada do Billing
            LLMCostMarginPercent = totalCost > 0 ? Math.Round((double)(totalCost / 2490.00m) * 100, 2) : 0,
            AverageLatencyMs = Math.Round(avgLatency, 0),
            DailyUsages = dailyUsages,
            AgentSummaries = agentSummaries,
            ModelSummaries = modelSummaries
        };

        return Result.Success(summary);
    }
}
