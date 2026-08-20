using FluentAssertions;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Tests.Domain;

public class SubscriptionTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _planId = Guid.NewGuid();

    [Fact]
    public void CreateTrialing_ShouldCreateSubscriptionInTrialingStatus()
    {
        // Act
        var subscriptionResult = Subscription.CreateTrialing(_tenantId, _planId, 14);

        // Assert
        subscriptionResult.IsSuccess.Should().BeTrue();
        var subscription = subscriptionResult.Value;
        subscription.TenantId.Should().Be(_tenantId);
        subscription.PlanId.Should().Be(_planId);
        subscription.Status.Should().Be(SubscriptionStatus.Trialing);
        subscription.CurrentPeriodEnd.Should().BeAfter(subscription.CurrentPeriodStart);
    }

    [Fact]
    public void Activate_ShouldTransitionToActiveAndSetStripeIds()
    {
        // Arrange
        var subscription = Subscription.CreateTrialing(_tenantId, _planId, 14).Value;
        var start = DateTime.UtcNow;
        var end = start.AddMonths(1);

        // Act
        var activateResult = subscription.Activate("cus_123", "sub_123", start, end);

        // Assert
        activateResult.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.StripeCustomerId.Should().Be("cus_123");
        subscription.StripeSubscriptionId.Should().Be("sub_123");
        subscription.CurrentPeriodStart.Should().Be(start);
        subscription.CurrentPeriodEnd.Should().Be(end);
    }

    [Fact]
    public void MarkPastDue_ShouldTransitionStatusToPastDue()
    {
        // Arrange
        var subscription = Subscription.CreateTrialing(_tenantId, _planId, 14).Value;

        // Act
        var result = subscription.MarkPastDue();

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.PastDue);
    }

    [Fact]
    public void Cancel_ShouldTransitionStatusToCanceled()
    {
        // Arrange
        var subscription = Subscription.CreateTrialing(_tenantId, _planId, 14).Value;

        // Act
        var result = subscription.Cancel(immediate: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);
    }
}
