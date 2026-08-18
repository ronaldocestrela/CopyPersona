using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Server.Components.Pages.Auth;

namespace PersonaScript.Server.UnitTests.Auth;

public class PasswordResetPageTests : BunitContext
{
    [Fact]
    public void EsqueciSenha_ShouldRenderFormAndTitle()
    {
        var cut = Render<EsqueciSenha>();

        cut.Find("h1").TextContent.Should().Be("Recuperar senha");
        cut.Find("a[href='/login']").TextContent.Should().Contain("Voltar ao login");
        cut.Find("form[action='/account/esqueci-senha']").Should().NotBeNull();
    }

    [Fact]
    public void EsqueciSenha_ShouldDisplaySuccessAlert_WhenQuerySuccess()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/esqueci-senha?success=true");

        var cut = Render<EsqueciSenha>();

        cut.Find(".auth-success-alert").TextContent.Should().Contain("E-mail enviado!");
    }

    [Fact]
    public void RedefinirSenha_ShouldRenderFormWithHiddenFields()
    {
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("/redefinir-senha?email=maria@example.com&token=sample-token");

        var cut = Render<RedefinirSenha>();

        cut.Find("h1").TextContent.Should().Be("Criar nova senha");
        cut.Find("form[action='/account/redefinir-senha']").Should().NotBeNull();
        cut.Find("input[name='Email']").GetAttribute("value").Should().Be("maria@example.com");
        cut.Find("input[name='Token']").GetAttribute("value").Should().Be("sample-token");
    }
}
