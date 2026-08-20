using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices;
using PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails;
using PersonaScript.Modules.Billing.Domain;
using Xunit;

namespace PersonaScript.Modules.Billing.Tests.Application;

public class GetSubscriptionDetailsAndInvoicesQueryHandlerTests
{
    private readonly ITenantContext _tenantContext;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUsageQuotaRepository _quotaRepository;
    private readonly IStripePaymentService _stripePaymentService;

    public GetSubscriptionDetailsAndInvoicesQueryHandlerTests()
    {
        _tenantContext = Substitute.For<ITenantContext>();
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _planRepository = Substitute.For<IPlanRepository>();
        _quotaRepository = Substitute.For<IUsageQuotaRepository>();
        _stripePaymentService = Substitute.For<IStripePaymentService>();
    }

    [Fact]
    public async Task GetSubscriptionDetailsQueryHandler_ShouldReturnDetails_WhenSubscriptionExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var plan = Plan.Create(PlanType.Pro, "Plano Pro", "Descrição Pro", 99m, 990m, 3, 30, 15).Value;
        var subscription = Subscription.CreateTrialing(tenantId, plan.Id).Value;
        subscription.Activate("cus_123", "sub_123", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));

        var quota = UsageQuota.Create(tenantId, subscription.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(30), 30, 3, 15).Value;


        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(subscription);
        _planRepository.GetByIdAsync(plan.Id, Arg.Any<CancellationToken>())
            .Returns(plan);
        _quotaRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(quota);
        _planRepository.GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Plan> { plan });

        var handler = new GetSubscriptionDetailsQueryHandler(
            _tenantContext, _subscriptionRepository, _planRepository, _quotaRepository);

        // Act
        var result = await handler.Handle(new GetSubscriptionDetailsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PlanName.Should().Be("Plano Pro");
        result.Value.Status.Should().Be(SubscriptionStatus.Active);
        result.Value.StripeCustomerId.Should().Be("cus_123");
        result.Value.ScriptsLimit.Should().Be(30);
        result.Value.AvailablePlans.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSubscriptionDetailsQueryHandler_ShouldReturnFailure_WhenSubscriptionNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var handler = new GetSubscriptionDetailsQueryHandler(
            _tenantContext, _subscriptionRepository, _planRepository, _quotaRepository);

        // Act
        var result = await handler.Handle(new GetSubscriptionDetailsQuery(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Subscription.NotFound.Code);
    }

    [Fact]
    public async Task GetSubscriptionDetailsQueryHandler_ShouldEnforceTenantIsolation()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        // Autenticado como Tenant A
        _tenantContext.TenantId.Returns(TenantId.From(tenantA));

        // Dados pertencentes ao Tenant B
        var plan = Plan.Create(PlanType.Pro, "Plano Pro", "Descrição", 99m, 990m, 3, 30, 15).Value;
        var subTenantB = Subscription.CreateTrialing(tenantB, plan.Id).Value;

        // O repositório só deve retornar subTenantB se consultado com tenantB. Para tenantA, retorna null.
        _subscriptionRepository.GetByTenantIdAsync(tenantA, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);
        _subscriptionRepository.GetByTenantIdAsync(tenantB, Arg.Any<CancellationToken>())
            .Returns(subTenantB);

        var handler = new GetSubscriptionDetailsQueryHandler(
            _tenantContext, _subscriptionRepository, _planRepository, _quotaRepository);

        // Act
        var result = await handler.Handle(new GetSubscriptionDetailsQuery(), CancellationToken.None);

        // Assert - Tenant A não consegue ver a assinatura do Tenant B
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(DomainErrors.Subscription.NotFound.Code);
    }

    [Fact]
    public async Task GetBillingInvoicesQueryHandler_ShouldReturnInvoices_WhenStripeCustomerExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var plan = Plan.Create(PlanType.Pro, "Plano Pro", "Descrição", 99m, 990m, 3, 30, 15).Value;
        var subscription = Subscription.CreateTrialing(tenantId, plan.Id).Value;
        subscription.Activate("cus_abc123", "sub_abc123", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));

        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        var invoices = new List<InvoiceDto>
        {
            new("inv_1", 99.00m, "BRL", "paid", "https://stripe.com/inv_1.pdf", DateTime.UtcNow)
        };

        _stripePaymentService.GetCustomerInvoicesAsync("cus_abc123", Arg.Any<CancellationToken>())
            .Returns(PersonaScript.BuildingBlocks.Results.Result.Success(invoices));

        var handler = new GetBillingInvoicesQueryHandler(
            _tenantContext, _subscriptionRepository, _stripePaymentService);

        // Act
        var result = await handler.Handle(new GetBillingInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().InvoiceId.Should().Be("inv_1");
    }

    [Fact]
    public async Task GetBillingInvoicesQueryHandler_ShouldReturnEmptyList_WhenNoStripeCustomer()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        _tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var plan = Plan.Create(PlanType.Basic, "Plano Básico", "Descrição", 0m, 0m, 1, 10, 5).Value;
        var subscription = Subscription.CreateTrialing(tenantId, plan.Id).Value; // Sem StripeCustomerId ainda

        _subscriptionRepository.GetByTenantIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(subscription);

        var handler = new GetBillingInvoicesQueryHandler(
            _tenantContext, _subscriptionRepository, _stripePaymentService);

        // Act
        var result = await handler.Handle(new GetBillingInvoicesQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
        await _stripePaymentService.DidNotReceive().GetCustomerInvoicesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
