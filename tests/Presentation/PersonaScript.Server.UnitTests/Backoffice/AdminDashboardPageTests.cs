using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminDashboardPageTests : BunitContext
{
    [Fact]
    public void Dashboard_WhenAuthorizedAsAdmin_ShouldRenderMetricsAndActionLinks()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetClaims(
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SystemAdmin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SupportAgent")
        );
        authContext.SetPolicies("RequireBackofficeAccess", "RequireSupportAgent", "RequireSystemAdmin");

        // Act
        var cut = Render<AdminDashboardPage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Dashboard Operacional");
        cut.Find(".space-y-8").TextContent.Should().Contain("Sistemas 100% Operacionais");
        cut.Find(".space-y-8").TextContent.Should().Contain("Assinantes Ativos");
        cut.Find(".space-y-8").TextContent.Should().Contain("Suporte ao Cliente & Tenants");
        cut.Find(".space-y-8").TextContent.Should().Contain("Prompts de IA & Engenharia de Contexto");
    }

    [Fact]
    public void Dashboard_WhenNotAuthorizedForSupport_ShouldShowBlockedStateForSupport()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("limited@personascript.ai");
        authContext.SetPolicies("RequireBackofficeAccess");

        // Act
        var cut = Render<AdminDashboardPage>();

        // Assert
        cut.Find(".space-y-8").TextContent.Should().Contain("Suporte ao Cliente (Bloqueado)");
        cut.Find(".space-y-8").TextContent.Should().Contain("Gestão de Prompts (Bloqueado)");
    }
}
