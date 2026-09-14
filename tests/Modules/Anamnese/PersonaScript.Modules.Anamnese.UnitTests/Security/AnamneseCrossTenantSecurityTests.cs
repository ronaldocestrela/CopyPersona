using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Security;

public class AnamneseCrossTenantSecurityTests
{
    private static AnamneseDbContext CreateDbContext(Guid tenantId, string dbName)
    {
        ITenantContext tenantContext = new FixedTenantContext(TenantId.From(tenantId));
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(new TenantDbContextInterceptor(tenantContext))
            .Options;

        return new AnamneseDbContext(options, tenantContext);
    }

    [Fact]
    public async Task Repository_GetByIdAsync_WhenCalledByForeignTenant_ShouldReturnNull()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();
        Guid anamneseIdA;

        // Tenant A cria a sua anamnese
        using (var dbA = CreateDbContext(tenantA, dbName))
        {
            var anamneseA = Domain.Anamnese.Create(tenantA).Value;
            anamneseA.UpdateEtapa1(new Etapa1QuemEVoce("Dra. Ana", "Ana", "Dermatologia", 8, "RQE", "Top 1", 30, MomentoAtualEnum.AgendaCheiaCobrarMais));
            await dbA.Anamneses.AddAsync(anamneseA);
            await dbA.SaveChangesAsync();
            anamneseIdA = anamneseA.Id;
        }

        // Act - Tenant B tenta acessar pelo ID direto do Tenant A
        using (var dbB = CreateDbContext(tenantB, dbName))
        {
            var repoB = new AnamneseRepository(dbB);
            var resultB = await repoB.GetByIdAsync(anamneseIdA);

            // Assert: Query Filter impede o vazamento e retorna null
            resultB.Should().BeNull();
        }
    }

    [Fact]
    public async Task Handlers_GetQueries_TenantBNaoDeveAcessarNemEtapasNemStatusDoTenantA()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var dbA = CreateDbContext(tenantA, dbName))
        {
            var tenantContextA = new FixedTenantContext(TenantId.From(tenantA));
            var repoA = new AnamneseRepository(dbA);
            var startHandlerA = new StartAnamneseCommandHandler(repoA, tenantContextA);
            await startHandlerA.Handle(new StartAnamneseCommand(), CancellationToken.None);

            var saveHandlerA = new SaveAnamneseStepCommandHandler(repoA, tenantContextA);
            await saveHandlerA.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dr. Carlos", "Carlos", "Cardio", 15, "Titulos", "Premio", 60, MomentoAtualEnum.AgendaCheiaCobrarMais)), CancellationToken.None);
        }

        // Act & Assert para Tenant B
        using (var dbB = CreateDbContext(tenantB, dbName))
        {
            var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));
            var repoB = new AnamneseRepository(dbB);

            var statusHandlerB = new GetAnamneseStatusQueryHandler(repoB, tenantContextB);
            var fullHandlerB = new GetFullAnamneseQueryHandler(repoB, tenantContextB);
            var stepHandlerB = new GetAnamneseStepQueryHandler(repoB, tenantContextB);

            var statusResult = await statusHandlerB.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);
            var fullResult = await fullHandlerB.Handle(new GetFullAnamneseQuery(), CancellationToken.None);
            var stepResult = await stepHandlerB.Handle(new GetAnamneseStepQuery(1), CancellationToken.None);

            statusResult.IsFailure.Should().BeTrue();
            statusResult.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);

            fullResult.IsFailure.Should().BeTrue();
            fullResult.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);

            stepResult.IsFailure.Should().BeTrue();
            stepResult.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);
        }
    }

    [Fact]
    public async Task CompleteAnamneseCommand_TenantBNaoDeveConcluirAnamneseDoTenantA()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var dbA = CreateDbContext(tenantA, dbName))
        {
            var anamneseA = Domain.Anamnese.Create(tenantA).Value;
            await dbA.Anamneses.AddAsync(anamneseA);
            await dbA.SaveChangesAsync();
        }

        // Tenant B tenta executar CompleteAnamneseCommand
        using (var dbB = CreateDbContext(tenantB, dbName))
        {
            var tenantContextB = new FixedTenantContext(TenantId.From(tenantB));
            var repoB = new AnamneseRepository(dbB);
            var completeHandlerB = new CompleteAnamneseCommandHandler(repoB, tenantContextB);

            var result = await completeHandlerB.Handle(new CompleteAnamneseCommand(), CancellationToken.None);

            // Assert: Tenant B não encontra anamnese para concluir
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);
        }

        // Tenant A continua com status Rascunho
        using (var dbA = CreateDbContext(tenantA, dbName))
        {
            var repoA = new AnamneseRepository(dbA);
            var anamneseA = await repoA.GetByTenantIdAsync();
            anamneseA.Should().NotBeNull();
            anamneseA!.Status.Should().Be(AnamneseStatus.Rascunho);
        }
    }
}
