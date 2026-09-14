using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class RegerarRoteiroModalTests : BunitContext
{
    [Fact]
    public void Modal_WhenNotVisible_ShouldNotRenderContent()
    {
        // Act
        var cut = Render<RegerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, false));

        // Assert
        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenVisible_ShouldRenderTitleAndInstructionTextarea()
    {
        // Act
        var cut = Render<RegerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ScriptId, Guid.NewGuid()));

        // Assert
        cut.Find(".modal-title").TextContent.Should().Contain("Regerar Roteiro com IA");
        cut.Find("textarea").Should().NotBeNull();
        cut.Find("button.btn-primary").TextContent.Should().Contain("Regerar Roteiro");
    }

    [Fact]
    public void Modal_WhenFeedbackIsEmpty_ShouldDisplayValidationErrorOnConfirm()
    {
        // Arrange
        var confirmed = false;
        var cut = Render<RegerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ScriptId, Guid.NewGuid())
            .Add(p => p.OnRegenerateRequested, _ => confirmed = true));

        // Act - Click confirm without typing feedback
        var confirmBtn = cut.Find("button.btn-primary");
        confirmBtn.Click();

        // Assert
        cut.Find(".alert-danger").TextContent.Should().Contain("Por favor, informe instruções");
        confirmed.Should().BeFalse();
    }

    [Fact]
    public void Modal_WhenFeedbackProvided_ShouldTriggerOnRegenerateRequestedCallback()
    {
        // Arrange
        string? emittedFeedback = null;
        var cut = Render<RegerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ScriptId, Guid.NewGuid())
            .Add(p => p.OnRegenerateRequested, fb => emittedFeedback = fb));

        // Act
        var textarea = cut.Find("textarea");
        textarea.Change("Tornar o gancho mais informal e adicionar CTA para Stories");

        var confirmBtn = cut.Find("button.btn-primary");
        confirmBtn.Click();

        // Assert
        emittedFeedback.Should().Be("Tornar o gancho mais informal e adicionar CTA para Stories");
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    [Fact]
    public void Modal_ClickingCloseOrCancel_ShouldTriggerOnClose()
    {
        // Arrange
        var closeCount = 0;
        var cut = Render<RegerarRoteiroModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.ScriptId, Guid.NewGuid())
            .Add(p => p.OnClose, () => closeCount++));

        // Act 1: Click close X
        cut.Find("button.btn-close").Click();

        // Act 2: Click Cancel
        cut.Find("button.btn-outline-secondary").Click();

        // Assert
        closeCount.Should().Be(2);
    }
}
