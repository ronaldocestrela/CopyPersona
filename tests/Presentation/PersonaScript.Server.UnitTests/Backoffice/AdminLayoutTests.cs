using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Server.Components.Layout.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminLayoutTests : BunitContext
{
    [Fact]
    public void AdminHeader_WhenUserIsAuthenticated_ShouldDisplayUserNameAndRole()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetRoles("SystemAdmin");

        // Act
        var cut = Render<AdminHeader>();

        // Assert
        cut.Markup.Should().Contain("admin@personascript.ai");
        cut.Markup.Should().Contain("SystemAdmin");
        cut.Markup.Should().Contain("Voltar ao App");
    }

    [Fact]
    public void AdminHeader_WhenUserIsNotAuthenticated_ShouldDisplayNotAuthenticatedText()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetNotAuthorized();

        // Act
        var cut = Render<AdminHeader>();

        // Assert
        cut.Markup.Should().Contain("Não autenticado");
    }
}
