using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Telemetry;

public sealed record GetAnomalyAlertsQuery(decimal CostThresholdUsd = 10.00m) : IQuery<IReadOnlyList<AnomalyAlertDto>>;

public sealed class GetAnomalyAlertsQueryHandler : IQueryHandler<GetAnomalyAlertsQuery, IReadOnlyList<AnomalyAlertDto>>
{
    private readonly IAgentExecutionLogRepository _repository;

    public GetAnomalyAlertsQueryHandler(IAgentExecutionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AnomalyAlertDto>>> Handle(GetAnomalyAlertsQuery query, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var logs = await _repository.GetLogsInPeriodAsync(startOfMonth, now, cancellationToken);

        var alerts = new List<AnomalyAlertDto>();

        // 1. Alert for Tenants exceeding monthly LLM cost threshold
        var tenantCosts = logs
            .GroupBy(x => x.TenantId)
            .Select(g => new { TenantId = g.Key, TotalCost = g.Sum(x => x.EstimatedCostUSD), ExecutionCount = g.Count() })
            .Where(x => x.TotalCost >= query.CostThresholdUsd);

        foreach (var tc in tenantCosts)
        {
            alerts.Add(new AnomalyAlertDto
            {
                AlertType = "HighTenantCost",
                Severity = "Critical",
                Title = "Consumo Elevado por Tenant",
                Description = $"Tenant {tc.TenantId} acumulou US$ {tc.TotalCost:F2} em custo de LLM neste mês ({tc.ExecutionCount} execuções).",
                TenantId = tc.TenantId,
                DetectedAtUtc = DateTime.UtcNow
            });
        }

        // 2. Alert for Models with high error rate (> 15%)
        var modelErrors = logs
            .GroupBy(x => x.ModelUsed)
            .Select(g => new
            {
                Model = g.Key,
                Total = g.Count(),
                Failures = g.Count(x => x.Status == AgentExecutionStatus.Failure)
            })
            .Where(x => x.Total >= 5 && (double)x.Failures / x.Total >= 0.15);

        foreach (var me in modelErrors)
        {
            var rate = (double)me.Failures / me.Total * 100;
            alerts.Add(new AnomalyAlertDto
            {
                AlertType = "ModelErrorSpike",
                Severity = "Warning",
                Title = "Taxa de Erros Elevada por Modelo",
                Description = $"O modelo '{me.Model}' apresentou {rate:F1}% de falhas ({me.Failures}/{me.Total}) no período.",
                TenantId = null,
                DetectedAtUtc = DateTime.UtcNow
            });
        }

        return Result.Success<IReadOnlyList<AnomalyAlertDto>>(alerts);
    }
}
