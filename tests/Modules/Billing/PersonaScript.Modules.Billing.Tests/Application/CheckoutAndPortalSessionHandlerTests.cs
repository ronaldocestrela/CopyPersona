using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.Commands.CreateCheckoutSession;
using PersonaScript.Modules.Billing.Application.Commands.CreateCustomerPortalSession;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Infrastructure.Repositories;

namespace PersonaScript.Modules.Billing.Tests.Application;

public class CheckoutAndPortalSessionHandlerTests
{
    private class TestTenantContext : ITenantContext
    {
        public TenantId TenantId { get; set; }
    }

    [Fact]
    public async Task CreateCheckoutSession_WithValidPlan_ShouldReturnSessionUrl()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext { TenantId = TenantId.From(tenantId) };
        var db = new BillingDbContext(options, tenantContext);

        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50, "price_pro_123").Value;
        db.Plans.Add(plan);
        db.SaveChanges();

        var stripeService = Substitute.For<IStripePaymentService>();
        stripeService.CreateCheckoutSessionAsync(
            tenantId,
            "user@example.com",
            Arg.Any<Plan>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success(new CheckoutSessionDto("cs_test_123", "https://checkout.stripe.com/pay/cs_test_123")));

        var handler = new CreateCheckoutSessionCommandHandler(
            new PlanRepository(db),
            stripeService,
            tenantContext);

        var command = new CreateCheckoutSessionCommand(PlanType.Pro, "user@example.com");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SessionId.Should().Be("cs_test_123");
        result.Value.CheckoutUrl.Should().Contain("cs_test_123");
    }

    [Fact]
    public async Task CreateCustomerPortalSession_WithActiveCustomer_ShouldReturnPortalUrl()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext { TenantId = TenantId.From(tenantId) };
        var db = new BillingDbContext(options, tenantContext);

        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value;
        db.Plans.Add(plan);

        var sub = Subscription.CreateTrialing(tenantId, plan.Id, 14).Value;
        sub.Activate("cus_stripe_999", "sub_stripe_999", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        db.Subscriptions.Add(sub);
        db.SaveChanges();

        var stripeService = Substitute.For<IStripePaymentService>();
        stripeService.CreateCustomerPortalSessionAsync(
            "cus_stripe_999",
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(Result.Success(new CustomerPortalDto("https://billing.stripe.com/p/session/test_portal_123")));

        var handler = new CreateCustomerPortalSessionCommandHandler(
            new SubscriptionRepository(db),
            stripeService,
            tenantContext);

        var command = new CreateCustomerPortalSessionCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PortalUrl.Should().Contain("test_portal_123");
    }

    [Fact]
    public async Task CreateCustomerPortalSession_WithoutCustomer_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext { TenantId = TenantId.From(tenantId) };
        var db = new BillingDbContext(options, tenantContext);

        var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value;
        db.Plans.Add(plan);

        var sub = Subscription.CreateTrialing(tenantId, plan.Id, 14).Value; // No customer ID set!
        db.Subscriptions.Add(sub);
        db.SaveChanges();

        var stripeService = Substitute.For<IStripePaymentService>();

        var handler = new CreateCustomerPortalSessionCommandHandler(
            new SubscriptionRepository(db),
            stripeService,
            tenantContext);

        var command = new CreateCustomerPortalSessionCommand();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Billing.Stripe.NoCustomer");
    }
}
