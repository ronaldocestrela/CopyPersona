using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;
using PersonaScript.Server.Components.Pages.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class CadastroPageTests : BunitContext
{
    public CadastroPageTests()
    {
        Services.AddSingleton(Substitute.For<ICommandHandler<RegisterUserCommand, Guid>>());
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Cadastro_ShouldRenderStitchTitleAndLoginLink()
    {
        var cut = Render<Cadastro>();

        cut.Find("h1").TextContent.Should().Be("Crie sua conta");
        cut.Find("a[href='/login']").TextContent.Should().Contain("Entre");
        cut.Find("button.auth-primary-btn").TextContent.Should().Contain("Criar Conta");
    }
}
