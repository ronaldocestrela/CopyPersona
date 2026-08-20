using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.BackgroundServices;

namespace PersonaScript.Modules.Billing.Tests.BackgroundServices;

public class MonthlyQuotaResetBackgroundServiceTests
{
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceScope _scope = Substitute.For<IServiceScope>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IUsageQuotaRepository _quotaRepository = Substitute.For<IUsageQuotaRepository>();
    private readonly ISubscriptionRepository _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly ILogger<MonthlyQuotaResetBackgroundService> _logger = Substitute.For<ILogger<MonthlyQuotaResetBackgroundService>>();

    public MonthlyQuotaResetBackgroundServiceTests()
    {
        _scopeFactory.CreateScope().Returns(_scope);
        _scope.ServiceProvider.Returns(_serviceProvider);
        _serviceProvider.GetService(typeof(IUsageQuotaRepository)).Returns(_quotaRepository);
        _serviceProvider.GetService(typeof(ISubscriptionRepository)).Returns(_subscriptionRepository);
        _serviceProvider.GetService(typeof(IPlanRepository)).Returns(_planRepository);
    }

    [Fact]
    public async Task ProcessQuotaResetsAsync_WhenExpiredQuotasExist_ShouldResetMonthlyQuotaWithPlanLimits()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        var oldStart = DateTime.UtcNow.AddMonths(-1).AddDays(-1);
        var oldEnd = oldStart.AddMonths(1);

        var quota = UsageQuota.Create(tenantId, subId, oldStart, oldEnd, scriptsLimit: 10, personasLimit: 2, aiAnalysesLimit: 15).Value;
        quota.ConsumeScript();
        quota.ConsumeAiAnalysis();

        _quotaRepository.GetExpiredQuotasAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<UsageQuota> { quota });

        var proPlan = Plan.Create(PlanType.Pro, "Pro Plan", "Desc", 97m, 970m, maxActivePersonas: 10, maxScriptsPerMonth: 50, maxAiAnalysesPerMonth: 100).Value;
        var subscription = Subscription.CreateTrialing(tenantId, proPlan.Id).Value;
        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(subscription);
        _planRepository.GetByIdAsync(proPlan.Id, Arg.Any<CancellationToken>()).Returns(proPlan);


        var service = new MonthlyQuotaResetBackgroundService(_scopeFactory, _logger);

        // Act
        var count = await service.ProcessQuotaResetsAsync(CancellationToken.None);

        // Assert
        count.Should().Be(1);
        quota.ScriptsGeneratedCount.Should().Be(0);
        quota.AiAnalysesCount.Should().Be(0);
        quota.ScriptsLimit.Should().Be(50);
        quota.ActivePersonasLimit.Should().Be(10);
        quota.AiAnalysesLimit.Should().Be(100);
        quota.PeriodEnd.Should().BeAfter(DateTime.UtcNow);
        _quotaRepository.Received(1).Update(quota);
    }

    [Fact]
    public async Task ProcessQuotaResetsAsync_WhenNoExpiredQuotas_ShouldReturnZero()
    {
        // Arrange
        _quotaRepository.GetExpiredQuotasAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<UsageQuota>());

        var service = new MonthlyQuotaResetBackgroundService(_scopeFactory, _logger);

        // Act
        var count = await service.ProcessQuotaResetsAsync(CancellationToken.None);

        // Assert
        count.Should().Be(0);
        _quotaRepository.DidNotReceive().Update(Arg.Any<UsageQuota>());
    }
}
