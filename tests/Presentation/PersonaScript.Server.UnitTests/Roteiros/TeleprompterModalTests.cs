using Bunit;
using FluentAssertions;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class TeleprompterModalTests : BunitContext
{
    public TeleprompterModalTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Modal_WhenIsVisibleFalse_ShouldNotRenderOverlay()
    {
        // Arrange
        var script = CreateScriptDto();

        // Act
        var cut = Render<TeleprompterModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Script, script));

        // Assert
        cut.FindAll(".teleprompter-overlay").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenIsVisibleTrue_ShouldRenderScriptContentAndControls()
    {
        // Arrange
        var script = CreateScriptDto();

        // Act
        var cut = Render<TeleprompterModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, script));

        // Assert
        cut.Find(".teleprompter-overlay").Should().NotBeNull();
        cut.Find(".teleprompter-text").TextContent.Should().Contain("Gancho impactante para o vídeo");
        cut.Find(".teleprompter-text").TextContent.Should().Contain("Desenvolvimento completo com retenção");
        cut.Find(".teleprompter-text").TextContent.Should().Contain("CTA para engajamento rápido");
        cut.Find("button").TextContent.Should().Contain("Iniciar");
    }

    private static VideoScriptDto CreateScriptDto()
    {
        return new VideoScriptDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Tema Teleprompter",
            "Educação",
            "Engajamento",
            "Gancho impactante para o vídeo",
            "Desenvolvimento completo com retenção",
            "CTA para engajamento rápido",
            "Legenda sugerida",
            "Dicas de gravação",
            "Empático",
            VideoScriptStatus.Draft,
            ScriptFeedbackRating.None,
            null,
            null,
            DateTimeOffset.UtcNow,
            null);
    }
}
