using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Commands;

public class SaveAnamneseStepCommandHandlerTests
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
    public async Task Handle_PrimeiroSalvamentoSemStartPrevio_DeveCriarRascunhoEAtualizarEtapa()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var handler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            var etapa1Dto = new Etapa1Dto(
                "Dra. Julia", "Julia", "Dermatologista", 6,
                "Especialização USP", "Prêmio Derma", 40, MomentoAtualEnum.AgendaRazoavel);

            var command = new SaveAnamneseStepCommand(1, Etapa1: etapa1Dto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var saved = await setup.Repository.GetByTenantIdAsync();
            saved.Should().NotBeNull();
            saved!.Etapa1.Should().NotBeNull();
            saved.Etapa1!.NomeCompleto.Should().Be("Dra. Julia");
            saved.PercentualConclusao.Should().Be(10);
            saved.EtapaAtual.Should().Be(2);
        }
    }

    [Fact]
    public async Task Handle_PreenchendoVariasEtapas_DeveCalcularPercentualEEtapaAtualCorretamente()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var handler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);

            // Salva Etapa 1
            await handler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dr. Leo", "Leo", "Nutri", 4, "CRN", "", 20, MomentoAtualEnum.IniciandoAgenda)), CancellationToken.None);

            // Salva Etapa 2
            await handler.Handle(new SaveAnamneseStepCommand(2, Etapa2: new Etapa2Dto("Transformar vidas", "Paciente A", "Inicio", "Superacao")), CancellationToken.None);

            // Assert
            var saved = await setup.Repository.GetByTenantIdAsync();
            saved.Should().NotBeNull();
            saved!.PercentualConclusao.Should().Be(20);
            saved.EtapaAtual.Should().Be(3);
        }
    }

    [Fact]
    public async Task Handle_EtapaInvalidaForaDoRange_DeveRetornarErroEtapaInvalida()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var handler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            var command = new SaveAnamneseStepCommand(11);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.EtapaInvalida);
        }
    }

    [Fact]
    public async Task Handle_EtapaSemDtoCorrespondente_DeveRetornarErroEtapaInvalida()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var handler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            var command = new SaveAnamneseStepCommand(1, Etapa1: null); // Etapa 1 sem Etapa1Dto

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.EtapaInvalida);
        }
    }
}
