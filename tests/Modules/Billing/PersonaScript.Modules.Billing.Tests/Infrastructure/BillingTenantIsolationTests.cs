using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;

namespace PersonaScript.Modules.Billing.Tests.Infrastructure;

public class BillingTenantIsolationTests
{
    private class TestTenantContext : ITenantContext
    {
        public TenantId TenantId { get; set; }
        public bool IsAuthenticated => TenantId.Value != Guid.Empty;
    }

    [Fact]
    public async Task BillingDbContext_GlobalQueryFilter_ShouldIsolateDataBetweenTenants()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var dbOptions = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var tenantContext = new TestTenantContext { TenantId = TenantId.From(tenantA) };

        // Seed data for Tenant A and Tenant B using un-filtered insertion or separate contexts
        using (var seedContext = new BillingDbContext(dbOptions, tenantContext))
        {
            var plan = Plan.Create(PlanType.Pro, "Pro Plan", "Desc", 97m, 970m, 5, 30, 50).Value;
            seedContext.Plans.Add(plan);

            var subA = Subscription.CreateTrialing(tenantA, plan.Id, 14).Value;
            var subB = Subscription.CreateTrialing(tenantB, plan.Id, 14).Value;

            var quotaA = UsageQuota.Create(tenantA, subA.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 30, 5, 50).Value;
            var quotaB = UsageQuota.Create(tenantB, subB.Id, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1), 30, 5, 50).Value;

            seedContext.Subscriptions.AddRange(subA, subB);
            seedContext.UsageQuotas.AddRange(quotaA, quotaB);
            await seedContext.SaveChangesAsync();
        }

        // Act & Assert for Tenant A
        tenantContext.TenantId = TenantId.From(tenantA);
        using (var contextA = new BillingDbContext(dbOptions, tenantContext))
        {
            var subs = await contextA.Subscriptions.ToListAsync();
            subs.Should().HaveCount(1);
            subs[0].TenantId.Should().Be(tenantA);

            var quotas = await contextA.UsageQuotas.ToListAsync();
            quotas.Should().HaveCount(1);
            quotas[0].TenantId.Should().Be(tenantA);
        }

        // Act & Assert for Tenant B
        tenantContext.TenantId = TenantId.From(tenantB);
        using (var contextB = new BillingDbContext(dbOptions, tenantContext))
        {
            var subs = await contextB.Subscriptions.ToListAsync();
            subs.Should().HaveCount(1);
            subs[0].TenantId.Should().Be(tenantB);

            var quotas = await contextB.UsageQuotas.ToListAsync();
            quotas.Should().HaveCount(1);
            quotas[0].TenantId.Should().Be(tenantB);
        }
    }
}
