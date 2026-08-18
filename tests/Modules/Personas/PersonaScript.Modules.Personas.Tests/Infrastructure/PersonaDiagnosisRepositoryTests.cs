using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Personas.Infrastructure.Repositories;

namespace PersonaScript.Modules.Personas.Tests.Infrastructure;

public class PersonaDiagnosisRepositoryTests
{
    private static PersonasDbContext CreateDbContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<PersonasDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new PersonasDbContext(options, tenantContext);
    }

    [Fact]
    public async Task AddAsync_And_GetByTenantIdAsync_ShouldIsolateDataBetweenTenants()
    {
        // Arrange
        var dbName = "PersonaTestDb_" + Guid.NewGuid();
        var tenantAGuid = Guid.NewGuid();
        var tenantBGuid = Guid.NewGuid();

        var tenantContextA = Substitute.For<ITenantContext>();
        tenantContextA.TenantId.Returns(TenantId.From(tenantAGuid));

        var tenantContextB = Substitute.For<ITenantContext>();
        tenantContextB.TenantId.Returns(TenantId.From(tenantBGuid));

        // Seed Tenant A Diagnosis
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            var diagnosisA = CreateDiagnosis(tenantAGuid, "Frase Posicionamento Tenant A");

            await repoA.AddAsync(diagnosisA);
            await repoA.SaveChangesAsync();
        }

        // Seed Tenant B Diagnosis
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var diagnosisB = CreateDiagnosis(tenantBGuid, "Frase Posicionamento Tenant B");

            await repoB.AddAsync(diagnosisB);
            await repoB.SaveChangesAsync();
        }

        // Act & Assert for Tenant A context
        using (var contextA = CreateDbContext(dbName, tenantContextA))
        {
            var repoA = new PersonaDiagnosisRepository(contextA);
            var resultA = await repoA.GetByTenantIdAsync();

            resultA.Should().NotBeNull();
            resultA!.TenantId.Should().Be(tenantAGuid);
            resultA.FrasePosicionamento.Should().Be("Frase Posicionamento Tenant A");
        }

        // Act & Assert for Tenant B context
        using (var contextB = CreateDbContext(dbName, tenantContextB))
        {
            var repoB = new PersonaDiagnosisRepository(contextB);
            var resultB = await repoB.GetByTenantIdAsync();

            resultB.Should().NotBeNull();
            resultB!.TenantId.Should().Be(tenantBGuid);
            resultB.FrasePosicionamento.Should().Be("Frase Posicionamento Tenant B");
        }
    }

    private static PersonaDiagnosis CreateDiagnosis(Guid tenantId, string frase)
    {
        var result = PersonaDiagnosis.Create(
            tenantId,
            Guid.NewGuid(),
            frase,
            "Síntese do perfil",
            new IdentidadeMarca("Tom", "Estilo", "Arq 1", "Arq 2"),
            new List<PilarConteudo> { new PilarConteudo("Educação", 100, "Desc", new[] { "Topico" }) },
            new MatrizRestricoes(new[] { "Tema" }, new[] { "Palavra" }, new[] { "Diretriz" }, "Limites")
        );

        return result.Value!;
    }
}
