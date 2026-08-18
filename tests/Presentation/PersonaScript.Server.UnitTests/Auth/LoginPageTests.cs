using Bunit;
using FluentAssertions;
using PersonaScript.Server.Components.Pages.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class LoginPageTests : BunitContext
{
    [Fact]
    public void Login_ShouldRenderStitchTitleAndRegisterLink()
    {
        var cut = Render<Login>();

        cut.Find("h1").TextContent.Should().Be("Bem-vindo de volta");
        cut.Find("a[href='/cadastro']").TextContent.Should().Contain("Cadastre-se");
        cut.Find("a[href='/esqueci-senha']").TextContent.Should().Contain("Esqueceu a senha");
        cut.Find("form[action='/account/login']").Should().NotBeNull();
    }
}
