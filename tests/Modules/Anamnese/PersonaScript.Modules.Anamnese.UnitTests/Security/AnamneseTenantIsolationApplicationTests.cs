using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Security;

public class AnamneseTenantIsolationApplicationTests
{
    private static AnamneseDbContext CreateDbContext(Guid tenantId, string dbName)
    {
        ITenantContext tenantContext = new FixedTenantContext(TenantId.From(tenantId));
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new AnamneseDbContext(options, tenantContext);
    }

    [Fact]
    public async Task TenantIsolation_TenantBNaoDeveVerNemAlterarAnamneseDoTenantA()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        // 1. Tenant A cria e salva etapa 1 da sua anamnese
        using (var dbContextA = CreateDbContext(tenantA, dbName))
        {
            var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
            var repoA = new AnamneseRepository(dbContextA);
            var startHandlerA = new StartAnamneseCommandHandler(repoA, tenantContextA);
            await startHandlerA.Handle(new StartAnamneseCommand(), CancellationToken.None);

            var saveHandlerA = new SaveAnamneseStepCommandHandler(repoA, tenantContextA);
            await saveHandlerA.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Tenant A Doctor", "DocA", "EspecialidadeA", 10, "TitulosA", "PrêmioA", 50, MomentoAtualEnum.AgendaCheiaCobrarMais)), CancellationToken.None);
        }

        // 2. Tenant B tenta consultar status e full anamnese
        using (var dbContextB = CreateDbContext(tenantB, dbName))
        {
            var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));
            var repoB = new AnamneseRepository(dbContextB);

            var getStatusHandlerB = new GetAnamneseStatusQueryHandler(repoB, tenantContextB);
            var getFullHandlerB = new GetFullAnamneseQueryHandler(repoB, tenantContextB);

            // Act
            var statusResultB = await getStatusHandlerB.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);
            var fullResultB = await getFullHandlerB.Handle(new GetFullAnamneseQuery(), CancellationToken.None);

            // Assert: Tenant B não encontra nenhuma anamnese (isolamento total)
            statusResultB.IsFailure.Should().BeTrue();
            statusResultB.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);

            fullResultB.IsFailure.Should().BeTrue();
            fullResultB.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);
        }

        // 3. Tenant B tenta salvar uma etapa: Isso deve criar uma NOVA anamnese isolada para Tenant B, sem afetar Tenant A
        using (var dbContextB = CreateDbContext(tenantB, dbName))
        {
            var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));
            var repoB = new AnamneseRepository(dbContextB);

            var saveHandlerB = new SaveAnamneseStepCommandHandler(repoB, tenantContextB);
            var resultSaveB = await saveHandlerB.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Tenant B Doctor", "DocB", "EspecialidadeB", 2, "TitulosB", "PrêmioB", 10, MomentoAtualEnum.IniciandoAgenda)), CancellationToken.None);

            resultSaveB.IsSuccess.Should().BeTrue();
        }

        // 4. Valida se Tenant A continua intacto com seus dados originais
        using (var dbContextA = CreateDbContext(tenantA, dbName))
        {
            var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
            var repoA = new AnamneseRepository(dbContextA);

            var getFullHandlerA = new GetFullAnamneseQueryHandler(repoA, tenantContextA);
            var fullResultA = await getFullHandlerA.Handle(new GetFullAnamneseQuery(), CancellationToken.None);

            fullResultA.IsSuccess.Should().BeTrue();
            fullResultA.Value.Etapa1.Should().NotBeNull();
            fullResultA.Value.Etapa1!.NomeCompleto.Should().Be("Tenant A Doctor");
        }
    }
}
