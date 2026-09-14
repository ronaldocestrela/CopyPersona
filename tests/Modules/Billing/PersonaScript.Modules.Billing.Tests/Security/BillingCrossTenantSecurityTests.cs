using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota;
using PersonaScript.Modules.Billing.Application.Commands.InitializeTenantSubscription;
using PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails;
using PersonaScript.Modules.Billing.Application.Queries.GetTenantQuotaUsage;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Billing.Tests.Security;

public class BillingCrossTenantSecurityTests
{
    private static BillingDbContext CreateDbContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new BillingDbContext(options, tenantContext);
    }

    [Fact]
    public async Task SubscriptionRepository_GetByTenantIdAsync_UnderForeignContext_ShouldReturnNull()
    {
        // Arrange
        var dbName = "BillingSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        // Seed Tenant A Subscription
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var plan = Plan.Create(PlanType.Pro, "Pro", "Desc", 97m, 970m, 5, 30, 50).Value!;
            contextA.Plans.Add(plan);
            await contextA.SaveChangesAsync();

            var subA = Subscription.CreateTrialing(tenantA, plan.Id, 14).Value!;
            contextA.Subscriptions.Add(subA);
            await contextA.SaveChangesAsync();
        }

        // Act - Tenant B tenta consultar pelo tenantA
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new SubscriptionRepository(contextB);
            var resultB = await repoB.GetByTenantIdAsync(tenantA);

            // Assert: Query Filter impede retorno de dados do Tenant A
            resultB.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetSubscriptionAndQuotaQueries_TenantBNaoDeveVerDadosDoTenantA()
    {
        // Arrange
        var dbName = "BillingSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var planRepoA = new PlanRepository(contextA);
            var subRepoA = new SubscriptionRepository(contextA);
            var quotaRepoA = new UsageQuotaRepository(contextA);

            var initHandlerA = new InitializeTenantSubscriptionCommandHandler(
                tenantContextA, planRepoA, subRepoA, quotaRepoA);

            await initHandlerA.Handle(new InitializeTenantSubscriptionCommand(PlanType.Pro), CancellationToken.None);
        }

        // Act & Assert sob Tenant B (sem assinatura ainda)
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var planRepoB = new PlanRepository(contextB);
            var subRepoB = new SubscriptionRepository(contextB);
            var quotaRepoB = new UsageQuotaRepository(contextB);

            var subDetailsHandlerB = new GetSubscriptionDetailsQueryHandler(
                tenantContextB, subRepoB, planRepoB, quotaRepoB);
            var quotaUsageHandlerB = new GetTenantQuotaUsageQueryHandler(
                tenantContextB, quotaRepoB);

            var subResult = await subDetailsHandlerB.Handle(new GetSubscriptionDetailsQuery(), CancellationToken.None);
            var quotaResult = await quotaUsageHandlerB.Handle(new GetTenantQuotaUsageQuery(), CancellationToken.None);

            subResult.IsFailure.Should().BeTrue();
            subResult.Error.Should().Be(DomainErrors.Subscription.NotFound);

            quotaResult.IsFailure.Should().BeTrue();
            quotaResult.Error.Should().Be(DomainErrors.UsageQuota.NotFound);
        }
    }

    [Fact]
    public async Task ConsumeQuotaCommand_TenantBConsumindoQuota_NaoDeveDebitarDoTenantA()
    {
        // Arrange
        var dbName = "BillingSecurity_" + Guid.NewGuid();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
        var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));

        // Inicializa assinaturas para ambos
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var initA = new InitializeTenantSubscriptionCommandHandler(
                tenantContextA, new PlanRepository(contextA), new SubscriptionRepository(contextA), new UsageQuotaRepository(contextA));
            await initA.Handle(new InitializeTenantSubscriptionCommand(PlanType.Basic), CancellationToken.None);
        }

        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var initB = new InitializeTenantSubscriptionCommandHandler(
                tenantContextB, new PlanRepository(contextB), new SubscriptionRepository(contextB), new UsageQuotaRepository(contextB));
            await initB.Handle(new InitializeTenantSubscriptionCommand(PlanType.Basic), CancellationToken.None);
        }

        // Act - Tenant B consome 3 roteiros
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var quotaRepoB = new UsageQuotaRepository(contextB);
            var txRepoB = new QuotaTransactionRepository(contextB);
            var consumeHandlerB = new ConsumeQuotaCommandHandler(tenantContextB, quotaRepoB, txRepoB);

            var result = await consumeHandlerB.Handle(
                new ConsumeQuotaCommand(QuotaResourceType.ScriptGeneration, Quantity: 3, "GeracaoDeVideo"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        // Assert - Tenant B consumiu 3
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var quotaRepoB = new UsageQuotaRepository(contextB);
            var quotaB = await quotaRepoB.GetByTenantIdAsync(tenantB);

            quotaB.Should().NotBeNull();
            quotaB!.ScriptsGeneratedCount.Should().Be(3);
        }

        // Assert - Tenant A permanece com 0 consumidos (isolamento estrito)
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var quotaRepoA = new UsageQuotaRepository(contextA);
            var quotaA = await quotaRepoA.GetByTenantIdAsync(tenantA);

            quotaA.Should().NotBeNull();
            quotaA!.ScriptsGeneratedCount.Should().Be(0);
        }
    }
}
