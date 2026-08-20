using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota;
using PersonaScript.Modules.Billing.Application.Commands.InitializeTenantSubscription;
using PersonaScript.Modules.Billing.Application.Queries.GetTenantQuotaUsage;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Tests.Application;

public class ApplicationHandlersTests
{
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IPlanRepository _planRepository = Substitute.For<IPlanRepository>();
    private readonly ISubscriptionRepository _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
    private readonly IUsageQuotaRepository _quotaRepository = Substitute.For<IUsageQuotaRepository>();
    private readonly IQuotaTransactionRepository _transactionRepository = Substitute.For<IQuotaTransactionRepository>();

    [Fact]
    public async Task InitializeTenantSubscription_WhenUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));
        var handler = new InitializeTenantSubscriptionCommandHandler(_tenantContext, _planRepository, _subscriptionRepository, _quotaRepository);

        // Act
        var result = await handler.Handle(new InitializeTenantSubscriptionCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Billing.TenantIdInvalid");
    }

    [Fact]
    public async Task InitializeTenantSubscription_WhenValid_ShouldCreateSubAndQuota()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));
        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((Subscription?)null);

        var plan = Plan.Create(PlanType.Basic, "Basic Plan", "Desc", 47m, 470m, 1, 10, 15).Value;
        _planRepository.GetByTypeAsync(PlanType.Basic, Arg.Any<CancellationToken>()).Returns(plan);

        var handler = new InitializeTenantSubscriptionCommandHandler(_tenantContext, _planRepository, _subscriptionRepository, _quotaRepository);

        // Act
        var result = await handler.Handle(new InitializeTenantSubscriptionCommand(PlanType.Basic), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _subscriptionRepository.Received(1).AddAsync(Arg.Is<Subscription>(s => s != null && s.TenantId == tenantId), Arg.Any<CancellationToken>());
        await _quotaRepository.Received(1).AddAsync(Arg.Is<UsageQuota>(q => q != null && q.TenantId == tenantId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConsumeQuota_WhenQuotaAvailable_ShouldConsumeAndUpdateRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var quota = UsageQuota.Create(tenantId, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 2, 20).Value;
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(quota);

        var handler = new ConsumeQuotaCommandHandler(_tenantContext, _quotaRepository, _transactionRepository);

        // Act
        var result = await handler.Handle(new ConsumeQuotaCommand(QuotaResourceType.ScriptGeneration, 1, "CreateScriptCommand"), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        quota.ScriptsGeneratedCount.Should().Be(1);
        _quotaRepository.Received(1).Update(quota);
        await _transactionRepository.Received(1).AddAsync(Arg.Is<QuotaTransaction>(t => t != null && t.TenantId == tenantId && t.ResourceType == QuotaResourceType.ScriptGeneration), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTenantQuotaUsage_ShouldReturnUsageDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var subId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var quota = UsageQuota.Create(tenantId, subId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 10, 2, 20).Value;
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(quota);

        var handler = new GetTenantQuotaUsageQueryHandler(_tenantContext, _quotaRepository);

        // Act
        var result = await handler.Handle(new GetTenantQuotaUsageQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.ScriptsLimit.Should().Be(10);
    }
}
