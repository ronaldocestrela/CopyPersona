using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Infrastructure;

public class TenantIsolationTests
{
    [Fact]
    public async Task QueryingScripts_ShouldOnlyReturnDataForAuthenticatedTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(TenantId.From(tenantA));

        var options = new DbContextOptionsBuilder<ScriptsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Seeding database directly with scripts for Tenant A and Tenant B
        Guid scriptBId;
        using (var seedContext = new ScriptsDbContext(options, tenantContext))
        {
            var scriptA = VideoScript.Create(
                tenantA, Guid.NewGuid(), Guid.NewGuid(),
                "Tema Tenant A", "Pilar A", "Objetivo A",
                "Gancho A", "Retenção A", "CTA A",
                "Legenda A", "Dicas A", "Tom A").Value;

            var scriptB = VideoScript.Create(
                tenantB, Guid.NewGuid(), Guid.NewGuid(),
                "Tema Tenant B", "Pilar B", "Objetivo B",
                "Gancho B", "Retenção B", "CTA B",
                "Legenda B", "Dicas B", "Tom B").Value;

            scriptBId = scriptB.Id;

            seedContext.VideoScripts.AddRange(scriptA, scriptB);
            await seedContext.SaveChangesAsync();
        }

        // Act - Querying with Tenant A authenticated
        using (var context = new ScriptsDbContext(options, tenantContext))
        {
            var repository = new VideoScriptRepository(context);
            var scripts = await repository.ListByTenantIdAsync();

            // Assert
            scripts.Should().HaveCount(1);
            scripts.Single().TenantId.Should().Be(tenantA);
            scripts.Single().Tema.Should().Be("Tema Tenant A");

            // Test Direct GetById for Tenant B script from Tenant A context
            var scriptBFromTenantA = await repository.GetByIdAsync(scriptBId);
            scriptBFromTenantA.Should().BeNull();
        }
    }

    [Fact]
    public async Task QueryingStoryPlanAndCalendar_ShouldOnlyReturnDataForAuthenticatedTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(TenantId.From(tenantA));

        var options = new DbContextOptionsBuilder<ScriptsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using (var seedContext = new ScriptsDbContext(options, tenantContext))
        {
            var planA = StoryPlan.Create(
                tenantA, Guid.NewGuid(), null, "3 stories/dia",
                new[] { new StoryBlock("Manhã", "08:00", "Gatilho", "Tipo A", "Exemplo A", "Objetivo A") },
                "Humanização A").Value;

            var planB = StoryPlan.Create(
                tenantB, Guid.NewGuid(), null, "5 stories/dia",
                new[] { new StoryBlock("Almoço", "12:00", "Gatilho", "Tipo B", "Exemplo B", "Objetivo B") },
                "Humanização B").Value;

            var calA = NinetyDayCalendar.Create(
                tenantA, Guid.NewGuid(), null, "Objetivo Tenant A",
                new[] { new WeeklyEditorialPlan(1, "Tema A", "Pilar A", "Obj A", "Formato A", new List<string> { "Ideia A" }) }).Value;

            var calB = NinetyDayCalendar.Create(
                tenantB, Guid.NewGuid(), null, "Objetivo Tenant B",
                new[] { new WeeklyEditorialPlan(1, "Tema B", "Pilar B", "Obj B", "Formato B", new List<string> { "Ideia B" }) }).Value;

            seedContext.StoryPlans.AddRange(planA, planB);
            seedContext.NinetyDayCalendars.AddRange(calA, calB);
            await seedContext.SaveChangesAsync();
        }

        // Act & Assert - Tenant A query
        using (var contextA = new ScriptsDbContext(options, tenantContext))
        {
            var storyRepoA = new StoryPlanRepository(contextA);
            var calRepoA = new NinetyDayCalendarRepository(contextA);

            var storyPlanResult = await storyRepoA.GetByTenantIdAsync();
            var calendarResult = await calRepoA.GetByTenantIdAsync();

            storyPlanResult.Should().NotBeNull();
            storyPlanResult!.TenantId.Should().Be(tenantA);
            storyPlanResult.FrequenciaDiariaRecomendada.Should().Be("3 stories/dia");

            calendarResult.Should().NotBeNull();
            calendarResult!.TenantId.Should().Be(tenantA);
            calendarResult.ObjetivoTrimestral.Should().Be("Objetivo Tenant A");
        }

        // Act & Assert - Switch context to Tenant B
        var tenantContextB = Substitute.For<ITenantContext>();
        tenantContextB.TenantId.Returns(TenantId.From(tenantB));

        using (var contextB = new ScriptsDbContext(options, tenantContextB))
        {
            var storyRepoB = new StoryPlanRepository(contextB);
            var calRepoB = new NinetyDayCalendarRepository(contextB);

            var storyPlanResult = await storyRepoB.GetByTenantIdAsync();
            var calendarResult = await calRepoB.GetByTenantIdAsync();

            storyPlanResult.Should().NotBeNull();
            storyPlanResult!.TenantId.Should().Be(tenantB);
            storyPlanResult.FrequenciaDiariaRecomendada.Should().Be("5 stories/dia");

            calendarResult.Should().NotBeNull();
            calendarResult!.TenantId.Should().Be(tenantB);
            calendarResult.ObjetivoTrimestral.Should().Be("Objetivo Tenant B");
        }
    }
}
