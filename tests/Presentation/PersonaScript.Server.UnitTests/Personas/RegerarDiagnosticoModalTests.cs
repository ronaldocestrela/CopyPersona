using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Pages.Posicionamento;
using Xunit;

namespace PersonaScript.Server.UnitTests.Personas;

public class RegerarDiagnosticoModalTests : BunitContext
{
    [Fact]
    public void Modal_WhenClosed_ShouldNotRenderBackdrop()
    {
        // Act
        var cut = Render<RegerarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, false));

        // Assert
        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenOpen_ShouldRenderFeedbackInputAndActionButtons()
    {
        // Act
        var cut = Render<RegerarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsLoading, false));

        // Assert
        cut.Find(".modal-header h2").TextContent.Should().Contain("Regerar Diagnóstico com IA");
        cut.Find("textarea").Should().NotBeNull();
        cut.Find("button.btn-primary").TextContent.Should().Contain("Confirmar e Regerar");
    }

    [Fact]
    public void Modal_WhenLoading_ShouldDisplaySpinnerAndDisableButtons()
    {
        // Act
        var cut = Render<RegerarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsLoading, true));

        // Assert
        cut.Find(".spinner").Should().NotBeNull();
        cut.Find(".modal-body").TextContent.Should().Contain("Gerando seu Diagnóstico Estratégico");
        cut.Find("button.btn-primary").HasAttribute("disabled").Should().BeTrue();
        cut.Find("button.btn-secondary").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void Modal_ConfirmingWithFeedback_ShouldTriggerOnConfirmedCallback()
    {
        // Arrange
        string? emittedFeedback = null;
        var cut = Render<RegerarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsLoading, false)
            .Add(p => p.OnConfirmed, fb => emittedFeedback = fb));

        // Act
        var textarea = cut.Find("textarea");
        textarea.Change("Dar mais ênfase ao atendimento humanizado e casos de implantes");

        var confirmBtn = cut.Find("button.btn-primary");
        confirmBtn.Click();

        // Assert
        emittedFeedback.Should().Be("Dar mais ênfase ao atendimento humanizado e casos de implantes");
    }

    [Fact]
    public void Modal_ClickingCloseOrCancel_ShouldTriggerOnClosed()
    {
        // Arrange
        var closedCount = 0;
        var cut = Render<RegerarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.IsLoading, false)
            .Add(p => p.OnClosed, () => closedCount++));

        // Act 1: Click close X
        cut.Find("button.modal-close-btn").Click();

        // Act 2: Click cancel
        cut.Find("button.btn-secondary").Click();

        // Assert
        closedCount.Should().Be(2);
    }
}
