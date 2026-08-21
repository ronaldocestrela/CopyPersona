using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class RoteirosPageTests : BunitContext
{
    private readonly IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>> _listQueryHandler;
    private readonly IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto> _statusQueryHandler;
    private readonly ICommandHandler<GenerateVideoScriptCommand, Guid> _generateCommandHandler;

    public RoteirosPageTests()
    {
        _listQueryHandler = Substitute.For<IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>>>();
        _statusQueryHandler = Substitute.For<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>>();
        _generateCommandHandler = Substitute.For<ICommandHandler<GenerateVideoScriptCommand, Guid>>();

        Services.AddSingleton(_listQueryHandler);
        Services.AddSingleton(_statusQueryHandler);
        Services.AddSingleton(_generateCommandHandler);
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Default mock for status query: Completed anamnese
        _statusQueryHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(
                Guid.NewGuid(),
                AnamneseStatus.Concluido,
                10,
                100,
                DateTimeOffset.UtcNow,
                null,
                DateTimeOffset.UtcNow))));
    }

    [Fact]
    public void Page_WhenAnamneseIsCompletedAndScriptsEmpty_ShouldRenderGenerateOptionInEmptyState()
    {
        // Arrange
        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(new List<VideoScriptDto>())));

        // Act
        var cut = Render<RoteirosPage>();

        // Assert
        cut.Find(".roteiros-container").TextContent.Should().Contain("Nenhum roteiro encontrado");
        cut.Find("button.btn-primary").TextContent.Should().Contain("Gerar Roteiro com IA");
    }

    [Fact]
    public void Page_WhenAnamneseIsNotCompletedAndScriptsEmpty_ShouldRenderAnamneseLinkInEmptyState()
    {
        // Arrange
        _statusQueryHandler.Handle(Arg.Any<GetAnamneseStatusQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new AnamneseStatusDto(
                Guid.NewGuid(),
                AnamneseStatus.Rascunho,
                3,
                30,
                DateTimeOffset.UtcNow,
                null,
                null))));

        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(new List<VideoScriptDto>())));

        // Act
        var cut = Render<RoteirosPage>();

        // Assert
        cut.Find(".roteiros-container").TextContent.Should().Contain("Conclua sua Anamnese para gerar seus primeiros roteiros");
        cut.Find(".py-5 a.btn-primary").TextContent.Should().Contain("Começar Anamnese");
    }

    [Fact]
    public void Page_WhenClickingGenerateButton_ShouldOpenGenerateModal()
    {
        // Arrange
        _listQueryHandler.Handle(Arg.Any<ListVideoScriptsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<VideoScriptDto>>(new List<VideoScriptDto>())));

        var cut = Render<RoteirosPage>();

        // Act
        var btn = cut.Find("button.btn-primary");
        btn.Click();

        // Assert
        cut.Find(".modal-title").TextContent.Should().Contain("Gerar Novo Roteiro com IA");
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
