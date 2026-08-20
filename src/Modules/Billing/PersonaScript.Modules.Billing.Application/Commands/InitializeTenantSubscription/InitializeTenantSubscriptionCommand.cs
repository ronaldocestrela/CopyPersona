using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Commands.InitializeTenantSubscription;

public record InitializeTenantSubscriptionCommand(PlanType TargetPlanType = PlanType.Basic) : ICommand<Guid>;

public sealed class InitializeTenantSubscriptionCommandHandler(
    ITenantContext tenantContext,
    IPlanRepository planRepository,
    ISubscriptionRepository subscriptionRepository,
    IUsageQuotaRepository quotaRepository)
    : ICommandHandler<InitializeTenantSubscriptionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(InitializeTenantSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var existingSub = await subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (existingSub != null)
        {
            return Result.Success(existingSub.Id);
        }

        var plan = await planRepository.GetByTypeAsync(command.TargetPlanType, cancellationToken);
        if (plan == null)
        {
            // Auto seed basic plan if not found during dev/test
            var planCreateResult = Plan.Create(
                command.TargetPlanType,
                command.TargetPlanType.ToString(),
                "Plano inicial",
                command.TargetPlanType == PlanType.Pro ? 97m : 47m,
                command.TargetPlanType == PlanType.Pro ? 970m : 470m,
                maxActivePersonas: command.TargetPlanType == PlanType.Pro ? 5 : 1,
                maxScriptsPerMonth: command.TargetPlanType == PlanType.Pro ? 30 : 10,
                maxAiAnalysesPerMonth: command.TargetPlanType == PlanType.Pro ? 50 : 15);

            if (planCreateResult.IsFailure)
            {
                return Result.Failure<Guid>(planCreateResult.Error);
            }

            plan = planCreateResult.Value;
            await planRepository.AddAsync(plan, cancellationToken);
        }

        var subResult = Subscription.CreateTrialing(tenantId, plan.Id, trialDays: 14);
        if (subResult.IsFailure)
        {
            return Result.Failure<Guid>(subResult.Error);
        }

        var subscription = subResult.Value;
        await subscriptionRepository.AddAsync(subscription, cancellationToken);

        var quotaResult = UsageQuota.Create(
            tenantId,
            subscription.Id,
            subscription.CurrentPeriodStart,
            subscription.CurrentPeriodEnd,
            plan.MaxScriptsPerMonth,
            plan.MaxActivePersonas,
            plan.MaxAiAnalysesPerMonth);

        if (quotaResult.IsFailure)
        {
            return Result.Failure<Guid>(quotaResult.Error);
        }

        await quotaRepository.AddAsync(quotaResult.Value, cancellationToken);

        return Result.Success(subscription.Id);
    }
}
