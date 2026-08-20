using FluentAssertions;
using NSubstitute;
using PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class FinancialMetricsQueryHandlerTests
{
    private readonly ISubscriptionRepository _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly GetFinancialMetricsQueryHandler _handler;

    public FinancialMetricsQueryHandlerTests()
    {
        _handler = new GetFinancialMetricsQueryHandler(_subscriptionRepository, _planRepository);
    }

    [Fact]
    public async Task Handle_ShouldCalculateMetricsCorrectly()
    {
        // Arrange
        var planPro = Plan.Create(PlanType.Pro, "Pro", "Desc", 97.00m, 970.00m, 5, 30, 50).Value;
        var planStarter = Plan.Create(PlanType.Basic, "Starter", "Desc", 47.00m, 470.00m, 2, 10, 15).Value;

        _planRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Plan> { planPro, planStarter });

        var sub1 = Subscription.CreateTrialing(Guid.NewGuid(), planPro.Id).Value;
        sub1.Activate("cus_1", "sub_1", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        typeof(Subscription).GetProperty(nameof(Subscription.Plan))?.SetValue(sub1, planPro);

        var sub2 = Subscription.CreateTrialing(Guid.NewGuid(), planStarter.Id).Value;
        sub2.Activate("cus_2", "sub_2", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        typeof(Subscription).GetProperty(nameof(Subscription.Plan))?.SetValue(sub2, planStarter);

        var sub3 = Subscription.CreateTrialing(Guid.NewGuid(), planPro.Id).Value;
        sub3.MarkPastDue();
        typeof(Subscription).GetProperty(nameof(Subscription.Plan))?.SetValue(sub3, planPro);

        var sub4 = Subscription.CreateTrialing(Guid.NewGuid(), planPro.Id).Value;
        sub4.Cancel(true);
        typeof(Subscription).GetProperty(nameof(Subscription.Plan))?.SetValue(sub4, planPro);

        _subscriptionRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Subscription> { sub1, sub2, sub3, sub4 });

        // Act
        var result = await _handler.Handle(new GetFinancialMetricsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var metrics = result.Value;
        metrics.TotalSubscriptions.Should().Be(4);
        metrics.ActiveSubscriptions.Should().Be(2);
        metrics.PastDueSubscriptions.Should().Be(1);
        metrics.CanceledSubscriptions.Should().Be(1);
        metrics.Mrr.Should().Be(97.00m + 47.00m + 97.00m); // 241.00m
        metrics.Arr.Should().Be(metrics.Mrr * 12);
        metrics.ChurnRate.Should().Be(25.0); // 1 canceled out of 4 total = 25%
        metrics.TotalPastDueAmount.Should().Be(97.00m);
        metrics.PlanBreakdown.Should().HaveCount(2);
    }
}
