using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails;

public record GetSubscriptionDetailsQuery : IQuery<SubscriptionDetailsDto>;

public class GetSubscriptionDetailsQueryHandler(
    ITenantContext tenantContext,
    ISubscriptionRepository subscriptionRepository,
    IPlanRepository planRepository,
    IUsageQuotaRepository quotaRepository)
    : IQueryHandler<GetSubscriptionDetailsQuery, SubscriptionDetailsDto>
{
    public virtual async Task<Result<SubscriptionDetailsDto>> Handle(GetSubscriptionDetailsQuery query, CancellationToken cancellationToken)

    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<SubscriptionDetailsDto>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (subscription == null)
        {
            return Result.Failure<SubscriptionDetailsDto>(DomainErrors.Subscription.NotFound);
        }

        var currentPlan = await planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
        var quota = await quotaRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        var activePlans = await planRepository.GetAllActiveAsync(cancellationToken);

        var planDtos = activePlans.Select(p => new PlanDto(
            p.Id,
            p.PlanType,
            p.Name,
            p.Description,
            p.MonthlyPrice,
            p.YearlyPrice,
            p.MaxActivePersonas,
            p.MaxScriptsPerMonth,
            p.MaxAiAnalysesPerMonth)).ToList();

        var dto = new SubscriptionDetailsDto(
            SubscriptionId: subscription.Id,
            PlanId: subscription.PlanId,
            PlanType: currentPlan?.PlanType ?? PlanType.Basic,
            PlanName: currentPlan?.Name ?? "Plano Básico",
            MonthlyPrice: currentPlan?.MonthlyPrice ?? 0m,
            Status: subscription.Status,
            CurrentPeriodStart: subscription.CurrentPeriodStart,
            CurrentPeriodEnd: subscription.CurrentPeriodEnd,
            CancelAtPeriodEnd: subscription.CancelAtPeriodEnd,
            StripeCustomerId: subscription.StripeCustomerId,
            StripeSubscriptionId: subscription.StripeSubscriptionId,
            ScriptsGeneratedCount: quota?.ScriptsGeneratedCount ?? 0,
            ScriptsLimit: quota?.ScriptsLimit ?? (currentPlan?.MaxScriptsPerMonth ?? 10),
            ActivePersonasCount: quota?.ActivePersonasCount ?? 0,
            ActivePersonasLimit: quota?.ActivePersonasLimit ?? (currentPlan?.MaxActivePersonas ?? 1),
            AiAnalysesCount: quota?.AiAnalysesCount ?? 0,
            AiAnalysesLimit: quota?.AiAnalysesLimit ?? (currentPlan?.MaxAiAnalysesPerMonth ?? 5),
            LastQuotaResetAt: quota?.LastResetAt ?? subscription.CurrentPeriodStart,
            AvailablePlans: planDtos);

        return Result.Success(dto);
    }
}
