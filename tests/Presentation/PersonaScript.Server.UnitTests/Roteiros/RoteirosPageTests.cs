using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class RoteirosPageTests : BunitContext
{
    private readonly IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>> _listQueryHandler;

    public RoteirosPageTests()
    {
        _listQueryHandler = Substitute.For<IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>>>();
        Services.AddSingleton(_listQueryHandler);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Page_WhenScriptsAreEmpty_ShouldRenderEmptyState()
    {
        // Arrange
        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(new List<VideoScriptDto>())));

        // Act
        var cut = Render<RoteirosPage>();

        // Assert
        cut.Find(".roteiros-container").TextContent.Should().Contain("Nenhum roteiro encontrado");
        cut.Find("a.btn-primary").TextContent.Should().Contain("Novo Conteúdo");
    }

    [Fact]
    public void Page_WhenScriptsExist_ShouldRenderScriptCards()
    {
        // Arrange
        var scriptList = new List<VideoScriptDto>
        {
            CreateScriptDto("Como falar em público sem medo", VideoScriptStatus.Draft),
            CreateScriptDto("5 Dicas para Oratória de Sucesso", VideoScriptStatus.Approved)
        };

        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(scriptList)));

        // Act
        var cut = Render<RoteirosPage>();

        // Assert
        cut.FindAll(".roteiro-card").Should().HaveCount(2);
        cut.Find(".roteiros-grid").TextContent.Should().Contain("Como falar em público sem medo");
        cut.Find(".roteiros-grid").TextContent.Should().Contain("5 Dicas para Oratória de Sucesso");
    }

    [Fact]
    public void Page_WhenFilteringByStatus_ShouldCallQueryHandlerWithStatus()
    {
        // Arrange
        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(new List<VideoScriptDto>())));

        var cut = Render<RoteirosPage>();

        // Act - Click on "Aprovados" pill
        var approvedPill = cut.FindAll(".filter-pill").First(p => p.TextContent.Contains("Aprovados"))!;
        approvedPill.Click();

        // Assert
        _listQueryHandler.Received().Handle(
            Arg.Is<ListVideoScriptsQuery>(q => q != null && q.Status == VideoScriptStatus.Approved),
            Arg.Any<CancellationToken>());
    }

    private static VideoScriptDto CreateScriptDto(string tema, VideoScriptStatus status)
    {
        return new VideoScriptDto(
            Guid.NewGuid(),
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
            status,
            ScriptFeedbackRating.None,
            null,
            null,
            DateTimeOffset.UtcNow,
            null);
    }
}
