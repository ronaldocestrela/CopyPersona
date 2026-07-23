using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Server.Components.Pages.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class LoginPageTests : BunitContext
{
    public LoginPageTests()
    {
        Services.AddSingleton(Substitute.For<ICommandHandler<LoginUserCommand, LoginResult>>());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Login_ShouldRenderStitchTitleAndRegisterLink()
    {
        var cut = Render<Login>();

        cut.Find("h1").TextContent.Should().Be("Bem-vindo de volta");
        cut.Find("a[href='/cadastro']").TextContent.Should().Contain("Cadastre-se");
        cut.Find("a[href='/esqueci-senha']").TextContent.Should().Contain("Esqueceu a senha");
    }
}
