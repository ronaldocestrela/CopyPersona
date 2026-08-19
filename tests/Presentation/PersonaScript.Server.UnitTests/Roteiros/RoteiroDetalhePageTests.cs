using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Scripts.Application.Commands.RegenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.Commands.SubmitVideoScriptFeedback;
using PersonaScript.Modules.Scripts.Application.Commands.UpdateVideoScriptStatus;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class RoteiroDetalhePageTests : BunitContext
{
    private readonly IQueryHandler<GetVideoScriptByIdQuery, VideoScriptDto> _getByIdHandler;
    private readonly ICommandHandler<UpdateVideoScriptStatusCommand> _updateStatusHandler;
    private readonly ICommandHandler<SubmitVideoScriptFeedbackCommand> _submitFeedbackHandler;
    private readonly ICommandHandler<RegenerateVideoScriptCommand, Guid> _regenerateHandler;

    public RoteiroDetalhePageTests()
    {
        _getByIdHandler = Substitute.For<IQueryHandler<GetVideoScriptByIdQuery, VideoScriptDto>>();
        _updateStatusHandler = Substitute.For<ICommandHandler<UpdateVideoScriptStatusCommand>>();
        _submitFeedbackHandler = Substitute.For<ICommandHandler<SubmitVideoScriptFeedbackCommand>>();
        _regenerateHandler = Substitute.For<ICommandHandler<RegenerateVideoScriptCommand, Guid>>();

        Services.AddSingleton(_getByIdHandler);
        Services.AddSingleton(_updateStatusHandler);
        Services.AddSingleton(_submitFeedbackHandler);
        Services.AddSingleton(_regenerateHandler);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Page_WhenScriptExists_ShouldRenderTitleAndTabs()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var dto = CreateScriptDto(scriptId, "Técnicas Secretas de Oratória");

        _getByIdHandler.Handle(Arg.Is<GetVideoScriptByIdQuery>(q => q != null && q.ScriptId == scriptId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(dto)));

        // Act
        var cut = Render<RoteiroDetalhePage>(parameters => parameters.Add(p => p.Id, scriptId));

        // Assert
        cut.Find(".roteiro-detalhe-header").TextContent.Should().Contain("Técnicas Secretas de Oratória");
        cut.FindAll(".tab-button").Should().HaveCount(5);
        cut.Find(".tab-content-card").TextContent.Should().Contain("Você congela ao falar em público?");
    }

    [Fact]
    public void Page_WhenTabClicked_ShouldSwitchActiveTabContent()
    {
        // Arrange
        var scriptId = Guid.NewGuid();
        var dto = CreateScriptDto(scriptId, "Técnicas Secretas de Oratória");

        _getByIdHandler.Handle(Arg.Any<GetVideoScriptByIdQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(dto)));

        var cut = Render<RoteiroDetalhePage>(parameters => parameters.Add(p => p.Id, scriptId));

        // Act - Click CTA tab
        var ctaTab = cut.FindAll(".tab-button").First(b => b.TextContent.Contains("Chamada para Ação"))!;
        ctaTab.Click();

        // Assert
        cut.Find(".tab-content-card").TextContent.Should().Contain("Comente EU para receber o PDF.");
    }

    private static VideoScriptDto CreateScriptDto(Guid id, string tema)
    {
        return new VideoScriptDto(
            id,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            tema,
            "Educação & Esclarecimento",
            "Engajamento e Conexão",
            "Você congela ao falar em público?",
            "Explique as técnicas de respiração.",
            "Comente EU para receber o PDF.",
            "Legenda sugerida para o post.",
            "Fale pausadamente e olhe para a câmera.",
            "Empático",
            VideoScriptStatus.Draft,
            ScriptFeedbackRating.None,
            null,
            null,
            DateTimeOffset.UtcNow,
            null);
    }
}
