using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Billing.Domain;

public class Subscription : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public Plan? Plan { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }
    public DateTime CurrentPeriodEnd { get; private set; }
    public bool CancelAtPeriodEnd { get; private set; }
    public string? StripeCustomerId { get; private set; }
    public string? StripeSubscriptionId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Subscription() { }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<Subscription> CreateTrialing(Guid tenantId, Guid planId, int trialDays = 14)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Subscription>(Error.Validation("Subscription.TenantIdRequired", "O TenantId é obrigatório."));
        }

        var now = DateTime.UtcNow;
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanId = planId,
            Status = SubscriptionStatus.Trialing,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddDays(trialDays),
            CancelAtPeriodEnd = false,
            CreatedAt = now
        };

        return Result.Success(subscription);
    }

    public Result Activate(string stripeCustomerId, string stripeSubscriptionId, DateTime periodStart, DateTime periodEnd)
    {
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result MarkPastDue()
    {
        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Cancel(bool immediate = false)
    {
        if (immediate)
        {
            Status = SubscriptionStatus.Canceled;
        }
        else
        {
            CancelAtPeriodEnd = true;
        }
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result ChangePlan(Guid newPlanId, DateTime periodStart, DateTime periodEnd)
    {
        PlanId = newPlanId;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
