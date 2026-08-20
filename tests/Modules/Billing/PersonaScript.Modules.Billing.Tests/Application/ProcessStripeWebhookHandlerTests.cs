using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Infrastructure.Repositories;

namespace PersonaScript.Modules.Billing.Tests.Application;

public class ProcessStripeWebhookHandlerTests
{
    private class TestTenantContext : ITenantContext
    {
        public TenantId TenantId { get; set; }
    }

    private (BillingDbContext context, Subscription sub, Plan plan, UsageQuota quota, Guid tenantId) CreateTestDatabase()
    {
        var tenantId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext { TenantId = TenantId.From(tenantId) };
        var db = new BillingDbContext(options, tenantContext);

        var plan = Plan.Create(PlanType.Pro, "Pro Plan", "Desc", 97m, 970m, 5, 30, 50, "price_pro_123").Value;
        db.Plans.Add(plan);

        var sub = Subscription.CreateTrialing(tenantId, plan.Id, 14).Value;
        sub.Activate("cus_123", "sub_123", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        db.Subscriptions.Add(sub);

        var quota = UsageQuota.Create(tenantId, sub.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 30, 5, 50).Value;
        db.UsageQuotas.Add(quota);

        db.SaveChanges();

        return (db, sub, plan, quota, tenantId);
    }

    [Fact]
    public async Task Handle_SubscriptionUpdated_ShouldUpdatePeriodAndStatus()
    {
        // Arrange
        var (db, sub, plan, quota, tenantId) = CreateTestDatabase();
        var handler = new ProcessStripeWebhookCommandHandler(
            new SubscriptionRepository(db),
            new PlanRepository(db),
            new UsageQuotaRepository(db),
            new ProcessedStripeEventRepository(db));

        var newStart = DateTime.UtcNow;
        var newEnd = DateTime.UtcNow.AddMonths(1);
        var command = new ProcessStripeWebhookCommand(
            EventId: "evt_sub_upd_1",
            EventType: "customer.subscription.updated",
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            StripePriceId: "price_pro_123",
            PeriodStart: newStart,
            PeriodEnd: newEnd,
            TenantIdMetadata: tenantId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSub = await db.Subscriptions.FirstAsync(s => s.Id == sub.Id);
        updatedSub.Status.Should().Be(SubscriptionStatus.Active);
        updatedSub.CurrentPeriodStart.Should().BeCloseTo(newStart, TimeSpan.FromSeconds(2));
        updatedSub.CurrentPeriodEnd.Should().BeCloseTo(newEnd, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Handle_PaymentFailed_ShouldMarkSubscriptionAsPastDue()
    {
        // Arrange
        var (db, sub, plan, quota, tenantId) = CreateTestDatabase();
        var handler = new ProcessStripeWebhookCommandHandler(
            new SubscriptionRepository(db),
            new PlanRepository(db),
            new UsageQuotaRepository(db),
            new ProcessedStripeEventRepository(db));

        var command = new ProcessStripeWebhookCommand(
            EventId: "evt_pay_fail_1",
            EventType: "invoice.payment_failed",
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            TenantIdMetadata: tenantId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSub = await db.Subscriptions.FirstAsync(s => s.Id == sub.Id);
        updatedSub.Status.Should().Be(SubscriptionStatus.PastDue);
    }

    [Fact]
    public async Task Handle_SubscriptionDeleted_ShouldCancelSubscription()
    {
        // Arrange
        var (db, sub, plan, quota, tenantId) = CreateTestDatabase();
        var handler = new ProcessStripeWebhookCommandHandler(
            new SubscriptionRepository(db),
            new PlanRepository(db),
            new UsageQuotaRepository(db),
            new ProcessedStripeEventRepository(db));

        var command = new ProcessStripeWebhookCommand(
            EventId: "evt_sub_del_1",
            EventType: "customer.subscription.deleted",
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            TenantIdMetadata: tenantId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedSub = await db.Subscriptions.FirstAsync(s => s.Id == sub.Id);
        updatedSub.Status.Should().Be(SubscriptionStatus.Canceled);
    }

    [Fact]
    public async Task Handle_DuplicateEventId_ShouldBeIdempotent()
    {
        // Arrange
        var (db, sub, plan, quota, tenantId) = CreateTestDatabase();
        var handler = new ProcessStripeWebhookCommandHandler(
            new SubscriptionRepository(db),
            new PlanRepository(db),
            new UsageQuotaRepository(db),
            new ProcessedStripeEventRepository(db));

        var command = new ProcessStripeWebhookCommand(
            EventId: "evt_duplicate_100",
            EventType: "invoice.payment_failed",
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            TenantIdMetadata: tenantId);

        // Act 1: First execution
        var result1 = await handler.Handle(command, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();
        (await db.Subscriptions.FirstAsync(s => s.Id == sub.Id)).Status.Should().Be(SubscriptionStatus.PastDue);

        // Reset subscription status to Active manually in DB
        sub.Activate("cus_123", "sub_123", DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        db.SaveChanges();

        // Act 2: Second execution with the same EventId
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert 2: Should succeed idempotently without changing status back to PastDue
        result2.IsSuccess.Should().BeTrue();
        (await db.Subscriptions.FirstAsync(s => s.Id == sub.Id)).Status.Should().Be(SubscriptionStatus.Active);
    }
}
