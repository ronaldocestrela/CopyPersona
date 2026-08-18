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
    private readonly ICommandHandler<StartAnamneseCommand, Guid> _startHandler;
    private readonly ICommandHandler<SaveAnamneseStepCommand> _saveHandler;
    private readonly ICommandHandler<CompleteAnamneseCommand> _completeHandler;

    public AnamneseWizardTests()
    {
        _statusHandler = Substitute.For<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        _fullHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        _startHandler = Substitute.For<ICommandHandler<StartAnamneseCommand, Guid>>();
        _saveHandler = Substitute.For<ICommandHandler<SaveAnamneseStepCommand>>();
        _completeHandler = Substitute.For<ICommandHandler<CompleteAnamneseCommand>>();

        Services.AddSingleton(_statusHandler);
        Services.AddSingleton(_fullHandler);
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
    public void Wizard_ShouldShowCompletedStateWhenStatusIsCompleted()
    {
        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Concluido, 10, 100, DateTimeOffset.UtcNow, null, DateTimeOffset.UtcNow);
        _statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                      .Returns(Task.FromResult(Result.Success(statusDto)));

        var cut = Render<AnamneseWizard>();

        cut.Find("h2").TextContent.Should().Contain("Anamnese Concluída com Sucesso");
        cut.Find("a[href='/posicionamento']").TextContent.Should().Contain("Ver Diagnóstico");
    }
}
