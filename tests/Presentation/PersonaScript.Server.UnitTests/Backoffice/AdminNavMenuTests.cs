using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Layout.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminNavMenuTests : BunitContext
{
    [Fact]
    public void AdminNavMenu_ShouldRenderBrandAndAllSevenNavigationLinks()
    {
        // Act
        var cut = Render<AdminNavMenu>();

        // Assert - Brand
        cut.Markup.Should().Contain("PersonaScript");
        cut.Markup.Should().Contain("BACKOFFICE");

        // Assert - Navigation links
        var links = cut.FindAll("a").Select(l => l.GetAttribute("href")).ToList();

        links.Should().Contain(new[]
        {
            "admin",
            "admin/tenants",
            "admin/prompts",
            "admin/telemetria",
            "admin/financeiro",
            "admin/conselhos-eticos",
            "admin/auditoria"
        });
    }

    [Fact]
    public void AdminNavMenu_ShouldDisplayRBACStatusBadge()
    {
        // Act
        var cut = Render<AdminNavMenu>();

        // Assert
        cut.Markup.Should().Contain("Proteção RBAC");
        cut.Markup.Should().Contain("Ativa");
    }
}
