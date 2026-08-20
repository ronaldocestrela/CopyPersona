namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public record PlanFinancialSummaryDto(
    Guid PlanId,
    string PlanType,
    string Name,
    decimal MonthlyPrice,
    int ActiveSubscribersCount,
    decimal MonthlyRevenue);

public record FinancialMetricsDto(
    decimal Mrr,
    decimal Arr,
    int TotalSubscriptions,
    int ActiveSubscriptions,
    int TrialingSubscriptions,
    int PastDueSubscriptions,
    int CanceledSubscriptions,
    double ChurnRate,
    decimal TotalPastDueAmount,
    IReadOnlyList<PlanFinancialSummaryDto> PlanBreakdown);
