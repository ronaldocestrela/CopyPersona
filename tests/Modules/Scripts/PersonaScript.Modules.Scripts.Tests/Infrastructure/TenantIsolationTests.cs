using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;
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
        }
    }
}
