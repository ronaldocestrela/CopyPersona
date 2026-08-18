using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Server.Components.Pages.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class CadastroPageTests : BunitContext
{
    [Fact]
    public void Cadastro_ShouldRenderStitchTitleAndLoginLink()
    {
        var cut = Render<Cadastro>();

        cut.Find("h1").TextContent.Should().Be("Crie sua conta");
        cut.Find("a[href='/login']").TextContent.Should().Contain("Entre");
        cut.Find("form[action='/account/register']").Should().NotBeNull();
        cut.Find("button.auth-primary-btn").TextContent.Should().Contain("Criar Conta");
    }

    [Fact]
    public void Cadastro_ShouldDisplayErrorFromQueryString()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/cadastro?error=Este%20e-mail%20j%C3%A1%20est%C3%A1%20cadastrado.");

        var cut = Render<Cadastro>();

        cut.Find(".auth-alert-error").TextContent.Should().Contain("Este e-mail já está cadastrado.");
    }
}
