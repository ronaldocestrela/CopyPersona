using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Server.Components.Pages.Posicionamento;
using Xunit;

namespace PersonaScript.Server.UnitTests.Personas;

public class PosicionamentoPageTests : BunitContext
{
    [Fact]
    public void Page_OnInitialized_ShouldRedirectToDiagnostico()
    {
        // Arrange
        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        Render<PosicionamentoPage>();

        // Assert
        navMan.Uri.Should().EndWith("/posicionamento/diagnostico");
    }
}
