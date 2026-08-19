using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;
using PersonaScript.Server.Components.Pages.Posicionamento;
using Xunit;

namespace PersonaScript.Server.UnitTests.Personas;

public class PosicionamentoDiagnosticoPageTests : BunitContext
{
    private readonly IQueryHandler<GetPersonaDiagnosisQuery, PersonaDiagnosisDto?> _getDiagnosisHandler;
    private readonly IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto> _getAnamneseStatusHandler;
    private readonly ICommandHandler<GeneratePersonaDiagnosisCommand, Guid> _generateHandler;
    private readonly ICommandHandler<UpdatePersonaDiagnosisCommand, Guid> _updateHandler;

    public PosicionamentoDiagnosticoPageTests()
    {
        _getDiagnosisHandler = Substitute.For<IQueryHandler<GetPersonaDiagnosisQuery, PersonaDiagnosisDto?>>();
        _getAnamneseStatusHandler = Substitute.For<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        _generateHandler = Substitute.For<ICommandHandler<GeneratePersonaDiagnosisCommand, Guid>>();
        _updateHandler = Substitute.For<ICommandHandler<UpdatePersonaDiagnosisCommand, Guid>>();

        Services.AddSingleton(_getDiagnosisHandler);
        Services.AddSingleton(_getAnamneseStatusHandler);
        Services.AddSingleton(_generateHandler);
        Services.AddSingleton(_updateHandler);
    }

    [Fact]
    public void Page_WhenAnamneseIsNotCompleted_ShouldShowAnamnesePendenteCard()
    {
        // Arrange
        _getAnamneseStatusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 3, 30, DateTimeOffset.UtcNow, null, null))));

        // Act
        var cut = Render<PosicionamentoDiagnosticoPage>();

        // Assert
        cut.Find(".empty-posicionamento-card").TextContent.Should().Contain("Anamnese Pendente");
    }

    [Fact]
    public void Page_WhenAnamneseCompletedButNoDiagnosis_ShouldShowGerarDiagnosticoCTA()
    {
        // Arrange
        _getAnamneseStatusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow))));

        _getDiagnosisHandler.Handle(Arg.Any<GetPersonaDiagnosisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<PersonaDiagnosisDto?>(Error.NotFound("Personas.DiagnosticoNaoEncontrado", "Não encontrado"))));

        // Act
        var cut = Render<PosicionamentoDiagnosticoPage>();

        // Assert
        cut.Find(".empty-posicionamento-card").TextContent.Should().Contain("Sua Anamnese está Concluída!");
        cut.Find("button").TextContent.Should().Contain("Gerar Diagnóstico com IA");
    }

    [Fact]
    public void Page_WhenDiagnosisExists_ShouldRenderDiagnosisDetails()
    {
        // Arrange
        var diagnosisDto = CreateDiagnosisDto();

        _getAnamneseStatusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow))));

        _getDiagnosisHandler.Handle(Arg.Any<GetPersonaDiagnosisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<PersonaDiagnosisDto?>(diagnosisDto)));

        // Act
        var cut = Render<PosicionamentoDiagnosticoPage>();

        // Assert
        cut.Find(".hero-frase").TextContent.Should().Contain("Autoridade em Odontologia Estética");
        cut.Find(".hero-sintese").TextContent.Should().Contain("Dr. Ronaldo atua com foco em sorrisos naturais");
        cut.FindAll(".pilar-card").Should().HaveCount(2);
        cut.Find(".pilar-card").TextContent.Should().Contain("Educação");
    }

    [Fact]
    public void Page_WhenClickingGerarDiagnosticoInicial_ShouldInvokeGenerateCommandHandler()
    {
        // Arrange
        _getAnamneseStatusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow))));

        _getDiagnosisHandler.Handle(Arg.Any<GetPersonaDiagnosisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<PersonaDiagnosisDto?>(Error.NotFound("Personas.DiagnosticoNaoEncontrado", "Não encontrado"))));

        _generateHandler.Handle(Arg.Any<GeneratePersonaDiagnosisCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(Guid.NewGuid())));

        var cut = Render<PosicionamentoDiagnosticoPage>();

        // Act
        cut.Find("button").Click();

        // Assert
        _generateHandler.Received(1).Handle(Arg.Any<GeneratePersonaDiagnosisCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Page_WhenGerarDiagnosticoFails_ShouldDisplayErrorMessage()
    {
        // Arrange
        _getAnamneseStatusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow))));

        _getDiagnosisHandler.Handle(Arg.Any<GetPersonaDiagnosisQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<PersonaDiagnosisDto?>(Error.NotFound("Personas.DiagnosticoNaoEncontrado", "Não encontrado"))));

        _generateHandler.Handle(Arg.Any<GeneratePersonaDiagnosisCommand>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Failure<Guid>(new Error("Personas.FalhaGeracaoLLM", "Falha ao se comunicar com a IA."))));

        var cut = Render<PosicionamentoDiagnosticoPage>();

        // Act
        cut.Find("button").Click();

        // Assert
        cut.Find(".empty-posicionamento-card").TextContent.Should().Contain("Falha ao se comunicar com a IA.");
    }


    private static PersonaDiagnosisDto CreateDiagnosisDto()
    {
        return new PersonaDiagnosisDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            FrasePosicionamento: "Autoridade em Odontologia Estética",
            SintesePerfil: "Dr. Ronaldo atua com foco em sorrisos naturais",
            IdentidadeMarca: new IdentidadeMarcaDto("Acolhedor", "Clean e elegante", "O Sábio", "O Cuidador"),
            PilaresConteudo: new List<PilarConteudoDto>
            {
                new PilarConteudoDto("Educação", 60, "Conteúdo educativo sobre sorrisos", new[] { "Mitos e verdades" }),
                new PilarConteudoDto("Bastidores", 40, "Dia a dia da clínica", new[] { "Bastidores dos procedimentos" })
            },
            MatrizRestricoes: new MatrizRestricoesDto(
                TemasProibidos: new[] { "Sensacionalismo" },
                PalavrasEvitar: new[] { "Garantia milagrosa" },
                DiretrizesInegociaveis: new[] { "Ética CRO" },
                LimitesExposicao: "Preservação da vida familiar"
            ),
            GeradoEm: DateTimeOffset.UtcNow,
            AtualizadoEm: null
        );
    }
}
