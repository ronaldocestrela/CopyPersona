using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics;

public record GetFinancialMetricsQuery : IQuery<FinancialMetricsDto>;

public sealed class GetFinancialMetricsQueryHandler(
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository) : IQueryHandler<GetFinancialMetricsQuery, FinancialMetricsDto>
{
    public async Task<Result<FinancialMetricsDto>> Handle(GetFinancialMetricsQuery query, CancellationToken cancellationToken)
    {
        var subscriptions = await subscriptionRepository.GetAllAsync(cancellationToken);
        var plans = await planRepository.GetAllAsync(cancellationToken);

        int totalSubscribers = subscriptions.Count;
        int activeCount = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
        int trialingCount = subscriptions.Count(s => s.Status == SubscriptionStatus.Trialing);
        int pastDueCount = subscriptions.Count(s => s.Status == SubscriptionStatus.PastDue);
        int canceledCount = subscriptions.Count(s => s.Status == SubscriptionStatus.Canceled);

        decimal mrr = subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing || s.Status == SubscriptionStatus.PastDue)
            .Sum(s => s.Plan?.MonthlyPrice ?? 0m);

        decimal arr = mrr * 12;

        double churnRate = totalSubscribers > 0
            ? Math.Round((double)canceledCount / totalSubscribers * 100, 2)
            : 0.0;

        decimal totalPastDueAmount = subscriptions
            .Where(s => s.Status == SubscriptionStatus.PastDue)
            .Sum(s => s.Plan?.MonthlyPrice ?? 0m);

        var planBreakdown = plans.Select(plan =>
        {
            var planSubs = subscriptions.Where(s => s.PlanId == plan.Id && s.Status == SubscriptionStatus.Active).ToList();
            int subCount = planSubs.Count;
            decimal revenue = subCount * plan.MonthlyPrice;

            return new PlanFinancialSummaryDto(
                plan.Id,
                plan.PlanType.ToString(),
                plan.Name,
                plan.MonthlyPrice,
                subCount,
                revenue);
        }).ToList();

        var metrics = new FinancialMetricsDto(
            mrr,
            arr,
            totalSubscribers,
            activeCount,
            trialingCount,
            pastDueCount,
            canceledCount,
            churnRate,
            totalPastDueAmount,
            planBreakdown);

        return Result.Success(metrics);
    }
}
