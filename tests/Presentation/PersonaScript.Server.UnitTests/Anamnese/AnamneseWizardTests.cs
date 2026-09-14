using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Server.Components.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class AnamneseWizardTests : BunitContext
{
    private readonly IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto> _statusHandler;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _fullHandler;
    private readonly IQueryHandler<AnalyzeStepClarificationQuery, ClarificationAnalysisResultDto> _clarificationHandler;
    private readonly ICommandHandler<StartAnamneseCommand, Guid> _startHandler;
    private readonly ICommandHandler<SaveAnamneseStepCommand> _saveHandler;
    private readonly ICommandHandler<CompleteAnamneseCommand> _completeHandler;

    public AnamneseWizardTests()
    {
        _statusHandler = Substitute.For<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        _fullHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        _clarificationHandler = Substitute.For<IQueryHandler<AnalyzeStepClarificationQuery, ClarificationAnalysisResultDto>>();
        _startHandler = Substitute.For<ICommandHandler<StartAnamneseCommand, Guid>>();
        _saveHandler = Substitute.For<ICommandHandler<SaveAnamneseStepCommand>>();
        _completeHandler = Substitute.For<ICommandHandler<CompleteAnamneseCommand>>();

        _clarificationHandler.Handle(Arg.Any<AnalyzeStepClarificationQuery>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(Result.Success(new ClarificationAnalysisResultDto(false, new List<ClarificationItemDto>()))));

        Services.AddSingleton(_statusHandler);
        Services.AddSingleton(_fullHandler);
        Services.AddSingleton(_clarificationHandler);
        Services.AddSingleton(_startHandler);
        Services.AddSingleton(_saveHandler);
        Services.AddSingleton(_completeHandler);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Wizard_ShouldStartNewAnamneseWhenNoDraftExists()
    {
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Failure<AnamneseStatusDto>(Error.NotFound("Anamnese.NotFound", "Não encontrada"))));

        _startHandler.Handle(Arg.Any<StartAnamneseCommand>(), Arg.Any<CancellationToken>())
                     .Returns(Task.FromResult(Result.Success(Guid.NewGuid())));

        var cut = Render<AnamneseWizard>();

        cut.Find("h1").TextContent.Should().Contain("Anamnese do Posicionamento Digital");
        cut.Find(".anamnese-progress-box").TextContent.Should().Contain("Etapa 1 de 10");

        _startHandler.Received(1).Handle(Arg.Any<StartAnamneseCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Wizard_ShouldNavigateAndSaveStepWhenNextClicked()
    {
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 1, 10, DateTimeOffset.UtcNow, null, null);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        _fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));

        _saveHandler.Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success()));

        var cut = Render<AnamneseWizard>();

        var nextBtn = cut.Find("button:contains('Próxima Etapa')");
        nextBtn.Click();

        _saveHandler.Received(1).Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>());
        cut.Find(".anamnese-progress-box").TextContent.Should().Contain("Etapa 2 de 10");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Wizard_ShouldShowAIClarificationModalWhenVaguenessDetectedOnStep()
    {
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 3, 30, DateTimeOffset.UtcNow, null, null);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        _fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));

        _saveHandler.Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success()));

        var item = new ClarificationItemDto("3.5", "PorQueEscolhemVoce", "Sou dedicado", "Resposta genérica", "Aprofunde seu diferencial", "Qual seu diferencial na 1ª consulta?", "Exemplo: consulta 3D");
        _clarificationHandler.Handle(Arg.Any<AnalyzeStepClarificationQuery>(), Arg.Any<CancellationToken>())
                             .Returns(Task.FromResult(Result.Success(new ClarificationAnalysisResultDto(true, new List<ClarificationItemDto> { item }))));

        var cut = Render<AnamneseWizard>();

        var nextBtn = cut.Find("button:contains('Próxima Etapa')");
        nextBtn.Click();

        cut.Find(".anamnese-ai-modal-card").Should().NotBeNull();
        cut.Find(".anamnese-ai-title").TextContent.Should().Contain("Aprofunde seu diferencial");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Wizard_ShouldShowCompletedStateWhenStatusIsCompleted()
    {
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        var cut = Render<AnamneseWizard>();

        cut.Find("h2").TextContent.Should().Contain("Anamnese Concluída com Sucesso");
        cut.Find("a[href='/posicionamento']").TextContent.Should().Contain("Ver Diagnóstico");
    }

    [Fact]
    public void Wizard_OnStep10_ClickingComplete_ShouldCallCompleteHandlerAndTransitionToCompletedState()
    {
        // Arrange
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 10, 100, DateTimeOffset.UtcNow, null, null);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        _fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));

        _saveHandler.Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success()));

        _completeHandler.Handle(Arg.Any<CompleteAnamneseCommand>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromResult(Result.Success()));

        var cut = Render<AnamneseWizard>();

        // Assert - Step 10 renderiza botão de conclusão
        var completeBtn = cut.Find("button:contains('Concluir Anamnese')");
        completeBtn.Should().NotBeNull();

        // Act
        completeBtn.Click();

        // Assert
        _saveHandler.Received(1).Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>());
        _completeHandler.Received(1).Handle(Arg.Any<CompleteAnamneseCommand>(), Arg.Any<CancellationToken>());
        cut.Find("h2").TextContent.Should().Contain("Anamnese Concluída com Sucesso");
    }

    [Fact]
    public void Wizard_ClickingSaveAndResume_ShouldInvokeSaveAndDisplayMessage()
    {
        // Arrange
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 4, 40, DateTimeOffset.UtcNow, null, null);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        _fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));

        _saveHandler.Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success()));

        var cut = Render<AnamneseWizard>();

        // Act
        var saveBtn = cut.Find("button:contains('Salvar e Continuar Depois')");
        saveBtn.Click();

        // Assert
        _saveHandler.Received(1).Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>());
        cut.Find(".alert-success").TextContent.Should().Contain("salvo com sucesso");
    }

    [Fact]
    public void Wizard_WhenSaveFails_ShouldDisplayErrorMessage()
    {
        // Arrange
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 2, 20, DateTimeOffset.UtcNow, null, null);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        _fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));

        _saveHandler.Handle(Arg.Any<SaveAnamneseStepCommand>(), Arg.Any<CancellationToken>())
                    .Returns(Task.FromResult(Result.Failure(Error.Validation("SaveError", "Erro ao gravar dados da etapa"))));

        var cut = Render<AnamneseWizard>();

        // Act
        var nextBtn = cut.Find("button:contains('Próxima Etapa')");
        nextBtn.Click();

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Erro ao gravar dados da etapa");
    }
}
