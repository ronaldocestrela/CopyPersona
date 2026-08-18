using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Commands;

public class CompleteAnamneseCommandHandlerTests
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
    public async Task Handle_ComEtapasIncompletas_DeveRetornarErroEtapasIncompletas()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var saveHandler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);
            await saveHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dra. Ana", "Ana", "Medica", 5, "", "", 10, MomentoAtualEnum.AgendaRazoavel)), CancellationToken.None);

            var completeHandler = new CompleteAnamneseCommandHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await completeHandler.Handle(new CompleteAnamneseCommand(), CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.EtapasIncompletas);
        }
    }

    [Fact]
    public async Task Handle_SemAnamneseExistente_DeveRetornarErroNaoEncontrada()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var completeHandler = new CompleteAnamneseCommandHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await completeHandler.Handle(new CompleteAnamneseCommand(), CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Anamnese.NaoEncontrada);
        }
    }

    [Fact]
    public async Task Handle_ComTodasAs10EtapasPreenchidas_DeveConcluirComSucesso()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var setup = CreateTestSetup(tenantId, dbName);
        using (setup.DbContext)
        {
            var saveHandler = new SaveAnamneseStepCommandHandler(setup.Repository, setup.TenantContext);

            await saveHandler.Handle(new SaveAnamneseStepCommand(1, Etapa1: new Etapa1Dto("Dr. Carlos", "Carlos", "Dentista", 8, "USP", "Top Dent", 50, MomentoAtualEnum.AgendaCheiaCobrarMais)), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(2, Etapa2: new Etapa2Dto("Motivacao", "Caso", "Fase", "Motor")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(3, Etapa3: new Etapa3Dto("Master", "Lucrativo", "Preferido", "Diferencial", "PorQue", "Critica")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(4, Etapa4: new Etapa4Dto("Perfil", "Medos", "Desejos", "Perguntas", "Mitos", CanalOrigemEnum.Instagram)), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(5, Etapa5: new Etapa5Dto("Perfis", "Admira", "NaoFaria", "Fora", "Atrai")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(6, Etapa6: new Etapa6Dto("Proibidos", "Vida", "Estilo", "Trabalho", NivelConfortoCameraEnum.SuperAVontade, "CRO")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(7, Etapa7: new Etapa7Dto("Temas", "Palestra", "Verdade", "Certo", "Errado", "Sonhos")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(8, Etapa8: new Etapa8Dto(new[] { ArquetipoComunicacaoEnum.Autoridade }, "Amostra", "Ok", "Nenhum")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(9, Etapa9: new Etapa9Dto("Rotina", "10h", "Apoio", "Video", "Postou")), CancellationToken.None);
            await saveHandler.Handle(new SaveAnamneseStepCommand(10, Etapa10: new Etapa10Dto("Meta 3m", "Meta 1a", "Passada", ResultadoPrioritarioEnum.MaisPacientesAgenda)), CancellationToken.None);

            var completeHandler = new CompleteAnamneseCommandHandler(setup.Repository, setup.TenantContext);

            // Act
            var result = await completeHandler.Handle(new CompleteAnamneseCommand(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var completed = await setup.Repository.GetByTenantIdAsync();
            completed.Should().NotBeNull();
            completed!.Status.Should().Be(AnamneseStatus.Concluido);
            completed.PercentualConclusao.Should().Be(100);
            completed.ConcluidoEm.Should().NotBeNull();
        }
    }
}
