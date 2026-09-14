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
using PersonaScript.Server.Components.Pages.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class AnamneseAuxiliaryComponentsTests : BunitContext
{
    [Fact]
    public void ProgressBar_ShouldRenderProgressInformationAndHandleStepClick()
    {
        // Arrange
        var clickedStep = 0;
        var cut = Render<AnamneseProgressBar>(parameters => parameters
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.TotalSteps, 10)
            .Add(p => p.PercentualConclusao, 30)
            .Add(p => p.Title, "Identificação Profissional")
            .Add(p => p.OnSelectStep, step => clickedStep = step));

        // Assert
        cut.Find(".anamnese-progress-box").TextContent.Should().Contain("Etapa 3 de 10 — Identificação Profissional");
        cut.Find(".anamnese-progress-box").TextContent.Should().Contain("30% concluído");

        var fillBar = cut.Find(".anamnese-progress-bar-fill");
        fillBar.GetAttribute("style").Should().Contain("width: 30%");

        var pills = cut.FindAll(".anamnese-stepper-pill");
        pills.Should().HaveCount(10);
        pills[0].ClassList.Should().Contain("completed");
        pills[1].ClassList.Should().Contain("completed");
        pills[2].ClassList.Should().Contain("active");

        // Act - Click step 5
        pills[4].Click();

        // Assert
        clickedStep.Should().Be(5);
    }

    [Theory]
    [InlineData(null, "Pendente", "short")]
    [InlineData("", "Pendente", "short")]
    [InlineData("Muito curto", "Resposta muito curta", "short")]
    [InlineData("Eu ajudo todo mundo a ter sucesso na vida com qualidade", "Resposta genérica — pode aprofundar", "short")]
    [InlineData("Atendo mulheres de 35 a 55 anos focando em harmonização orofacial e reabilitação estética com prótese sobre implante", "Boa resposta", "good")]
    public void CharacterCounter_ShouldDisplayCorrectBadgeStatus(string? inputText, string expectedBadgeText, string expectedClass)
    {
        // Act
        var cut = Render<AnamneseCharacterCounter>(parameters => parameters
            .Add(p => p.Text, inputText));

        // Assert
        var badge = cut.Find(".anamnese-counter-badge");
        badge.TextContent.Should().Contain(expectedBadgeText);
        badge.ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    public void DidacticTooltip_ShouldToggleContentVisibilityOnClick()
    {
        // Act
        var cut = Render<AnamneseDidacticTooltip>(parameters => parameters
            .Add(p => p.InitiallyOpen, true)
            .AddChildContent("<p class='tooltip-test-body'>Exemplo prático de dermatologia clínica</p>"));

        // Assert Initially Open
        cut.Find(".tooltip-test-body").TextContent.Should().Contain("Exemplo prático de dermatologia clínica");
        cut.Find(".anamnese-tooltip-title").TextContent.Should().Contain("▲ Ocultar");

        // Click header to toggle closed
        cut.Find(".anamnese-tooltip-title").Click();
        cut.FindAll(".tooltip-test-body").Should().BeEmpty();
        cut.Find(".anamnese-tooltip-title").TextContent.Should().Contain("▼ Ver exemplo");

        // Click header to toggle open again
        cut.Find(".anamnese-tooltip-title").Click();
        cut.Find(".tooltip-test-body").TextContent.Should().Contain("Exemplo prático de dermatologia clínica");
    }

    [Fact]
    public void AnamnesePage_ShouldRenderWithWizardComponent()
    {
        // Arrange
        var statusHandler = Substitute.For<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        var fullHandler = Substitute.For<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>>();
        var clarificationHandler = Substitute.For<IQueryHandler<AnalyzeStepClarificationQuery, ClarificationAnalysisResultDto>>();
        var startHandler = Substitute.For<ICommandHandler<StartAnamneseCommand, Guid>>();
        var saveHandler = Substitute.For<ICommandHandler<SaveAnamneseStepCommand>>();
        var completeHandler = Substitute.For<ICommandHandler<CompleteAnamneseCommand>>();

        var statusDto = new AnamneseStatusDto(Guid.NewGuid(), AnamneseStatus.Rascunho, 1, 10, DateTimeOffset.UtcNow, null, null);
        statusHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
                     .Returns(Task.FromResult(Result.Success(statusDto)));
        fullHandler.Handle(Arg.Any<GetFullAnamneseQuery>(), Arg.Any<CancellationToken>())
                   .Returns(Task.FromResult(Result.Success(new FullAnamneseDto(statusDto, null, null, null, null, null, null, null, null, null, null))));
        clarificationHandler.Handle(Arg.Any<AnalyzeStepClarificationQuery>(), Arg.Any<CancellationToken>())
                            .Returns(Task.FromResult(Result.Success(new ClarificationAnalysisResultDto(false, new List<ClarificationItemDto>()))));

        Services.AddSingleton(statusHandler);
        Services.AddSingleton(fullHandler);
        Services.AddSingleton(clarificationHandler);
        Services.AddSingleton(startHandler);
        Services.AddSingleton(saveHandler);
        Services.AddSingleton(completeHandler);

        // Act
        var cut = Render<AnamnesePage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Anamnese do Posicionamento Digital");
    }
}
