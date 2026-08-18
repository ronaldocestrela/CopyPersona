using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests;

public class AnamnesePersistenceTests
{
    private static AnamneseDbContext CreateDbContext(Guid tenantId, string dbName)
    {
        ITenantContext tenantContext = new FixedTenantContext(TenantId.From(tenantId));

        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AnamneseDbContext(options, tenantContext);
    }

    [Fact]
    public async Task AddAndGet_ComTenantValido_DeveSalvarERecuperarAnamneseComSucesso()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var dbContext = CreateDbContext(tenantId, dbName))
        {
            var repository = new AnamneseRepository(dbContext);
            var anamnese = Domain.Anamnese.Create(tenantId).Value;
            anamnese.UpdateEtapa1(new Etapa1QuemEVoce("Dra. Mari", "Mari", "Dentista", 5, "Pós", "Prêmio", 30, MomentoAtualEnum.AgendaRazoavel));

            await repository.AddAsync(anamnese);
            await repository.SaveChangesAsync();
        }

        // Act & Assert
        using (var dbContext = CreateDbContext(tenantId, dbName))
        {
            var repository = new AnamneseRepository(dbContext);
            var recuperada = await repository.GetByTenantIdAsync();

            recuperada.Should().NotBeNull();
            recuperada!.TenantId.Should().Be(tenantId);
            recuperada.Etapa1.Should().NotBeNull();
            recuperada.Etapa1!.NomeCompleto.Should().Be("Dra. Mari");
        }
    }

    [Fact]
    public async Task TenantIsolation_TenantANaoDeveAcessarDadosDoTenantB()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        // Tenant A cria sua Anamnese
        using (var dbContextA = CreateDbContext(tenantA, dbName))
        {
            var repositoryA = new AnamneseRepository(dbContextA);
            var anamneseA = Domain.Anamnese.Create(tenantA).Value;
            anamneseA.UpdateEtapa1(new Etapa1QuemEVoce("Tenant A User", "A", "Medico", 10, "CRM", "Prêmio A", 50, MomentoAtualEnum.AgendaCheiaCobrarMais));

            await repositoryA.AddAsync(anamneseA);
            await repositoryA.SaveChangesAsync();
        }

        // Tenant B tenta consultar
        using (var dbContextB = CreateDbContext(tenantB, dbName))
        {
            var repositoryB = new AnamneseRepository(dbContextB);

            // Act
            var anamneseDoB = await repositoryB.GetByTenantIdAsync();
            var todasAsAnamneses = await dbContextB.Anamneses.ToListAsync();

            // Assert: O filtro global do EF Core impede que o Tenant B enxergue a anamnese do Tenant A
            anamneseDoB.Should().BeNull();
            todasAsAnamneses.Should().BeEmpty();
        }
    }
}
