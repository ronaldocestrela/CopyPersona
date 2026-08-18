using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Infrastructure;

public class AnamneseDbContextEtapa5Tests
{
    [Fact]
    public async Task EFCore_DeveSalvarERecuperarEtapa5ComMultiplosPerfisSemErro()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        var tenantContext = new FixedTenantContext(TenantId.From(tenantId));
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using var dbContext = new AnamneseDbContext(options, tenantContext);
        var repo = new AnamneseRepository(dbContext);

        var anamnese = Domain.Anamnese.Create(tenantId).Value;
        var etapa5 = new Etapa5SuasReferencias(
            new[] { "dramarianacosta", "drpedro" },
            "Didática e clareza",
            "Dancinhas",
            new[] { "marca1", "marca2" },
            "Estética minimalista"
        );

        anamnese.UpdateEtapa5(etapa5);
        await repo.AddAsync(anamnese, CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);

        // Act
        var retrieved = await repo.GetByTenantIdAsync(CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Etapa5.Should().NotBeNull();
        retrieved.Etapa5!.PerfisArea.Should().BeEquivalentTo(new[] { "dramarianacosta", "drpedro" });
        retrieved.Etapa5.PerfisForaArea.Should().BeEquivalentTo(new[] { "marca1", "marca2" });
    }
}
