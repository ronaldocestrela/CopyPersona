using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Queries;

public class GetAnamneseQueriesTests
{
    private static (AnamneseDbContext DbContext, AnamneseRepository Repository, FixedTenantContext TenantContext) CreateTestSetup(Guid tenantId, string dbName)
    {
        var tenantContext = new FixedTenantContext(TenantId.From(tenantId));
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var dbContext = new AnamneseDbContext(options, tenantContext);
        var repo = new AnamneseRepository(dbContext);
        return (dbContext, repo, tenantContext);
    }

    [Fact]
    public async Task GetAnamneseStatus_SemAnamnese_DeveRetornarErroNaoEncontrada()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var handler = new GetAnamneseStatusQueryHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await handler.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);
        }
    }

    [Fact]
    public async Task GetAnamneseStatus_ComAnamneseExistente_DeveRetornarStatusEProgresso()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var saveHandler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            await saveHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dra. Beatriz", "Bia", "Psiquiatra", 10, "USP", "Prêmio", 30, MomentoAtualEnum.AgendaCheiaCobrarMais)), CancellationToken.None);

            var queryHandler = new GetAnamneseStatusQueryHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await queryHandler.Handle(new GetAnamneseStatusQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.PercentualConclusao.Should().Be(10);
            result.Value.EtapaAtual.Should().Be(2);
            result.Value.Status.Should().Be(AnamneseStatus.Rascunho);
        }
    }

    [Fact]
    public async Task GetAnamneseStep_ParaEtapaPreenchida_DeveRetornarDtoDaEtapa()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var saveHandler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            await saveHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dr. Fernando", "Fer", "Cardio", 12, "FMRP-USP", "", 60, MomentoAtualEnum.ReferenciaExpansao)), CancellationToken.None);

            var queryHandler = new GetAnamneseStepQueryHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await queryHandler.Handle(new GetAnamneseStepQuery(1), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeOfType<Etapa1Dto>();
            var dto = (Etapa1Dto)result.Value!;
            dto.NomeCompleto.Should().Be("Dr. Fernando");
        }
    }

    [Fact]
    public async Task GetFullAnamnese_ComAnamneseExistente_DeveRetornarFullDtoComStatusEEtapas()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var saveHandler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            await saveHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dra. Camila", "Cami", "Orto", 7, "", "", 25, MomentoAtualEnum.AgendaRazoavel)), CancellationToken.None);

            var queryHandler = new GetFullAnamneseQueryHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await queryHandler.Handle(new GetFullAnamneseQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Status.PercentualConclusao.Should().Be(10);
            result.Value.Etapa1.Should().NotBeNull();
            result.Value.Etapa1!.NomeCompleto.Should().Be("Dra. Camila");
            result.Value.Etapa2.Should().BeNull();
        }
    }
}
