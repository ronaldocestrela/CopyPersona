using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class AuthSocialButtonsTests : BunitContext
{
    [Fact]
    public void AuthSocialButtons_ShouldRenderActiveGoogleAndAppleLinks()
    {
        var cut = Render<AuthSocialButtons>();

        var googleLink = cut.Find("a[href='/account/external-login/Google']");
        googleLink.Should().NotBeNull();
        googleLink.TextContent.Should().Contain("Google");

        var appleLink = cut.Find("a[href='/account/external-login/Apple']");
        appleLink.Should().NotBeNull();
        appleLink.TextContent.Should().Contain("Apple");
    }
}
