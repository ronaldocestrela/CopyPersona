using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Layout;
using Xunit;

namespace PersonaScript.Server.UnitTests.Layout;

public class NavMenuTests : BunitContext
{
    [Fact]
    public void NavMenu_ShouldRenderBrandTitleAndMainNavigationLinks()
    {
        // Act
        var cut = Render<NavMenu>();

        // Assert - Brand
        cut.Find(".brand-title").TextContent.Should().Contain("PersonaScript");

        // Assert - Navigation links
        var links = cut.FindAll("a.nav-link");
        links.Select(l => l.GetAttribute("href")).Should().Contain(new[]
        {
            "",
            "anamnese",
            "posicionamento/diagnostico",
            "roteiros",
            "backoffice"
        });
    }

    [Fact]
    public void NavMenu_ShouldIncludePhase4RoteirosLinkWithIcon()
    {
        // Act
        var cut = Render<NavMenu>();

        // Assert
        var roteirosLink = cut.FindAll("a.nav-link").FirstOrDefault(l => l.GetAttribute("href") == "roteiros");
        roteirosLink.Should().NotBeNull();
        roteirosLink!.TextContent.Should().Contain("Roteiros");
    }

    [Fact]
    public void NavMenu_WhenTogglingMobileMenu_ShouldToggleCssClass()
    {
        // Act
        var cut = Render<NavMenu>();
        var toggleBtn = cut.Find(".nav-toggle");

        // Initial state
        cut.Find(".nav-links-container").ClassList.Should().NotContain("active");

        // Click to open
        toggleBtn.Click();
        cut.Find(".nav-links-container").ClassList.Should().Contain("active");

        // Click to close
        toggleBtn.Click();
        cut.Find(".nav-links-container").ClassList.Should().NotContain("active");
    }
}
