using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonaScript.Server.UnitTests.Auth;

public sealed class UserPagesAuthorizationIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public UserPagesAuthorizationIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/anamnese")]
    [InlineData("/posicionamento/diagnostico")]
    [InlineData("/posicionamento")]
    [InlineData("/roteiros")]
    public async Task UserPages_ShouldRedirectToLogin_WhenUnauthenticated(string url)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/login?ReturnUrl={Uri.EscapeDataString(url)}");
    }

    [Theory]
    [InlineData("/anamnese")]
    [InlineData("/posicionamento/diagnostico")]
    [InlineData("/roteiros")]
    public async Task UserPages_ShouldReturnOk_WhenAuthenticated(string url)
    {
        var client = await AuthTestHelper.CreateAuthenticatedClientAsync(_factory);

        using var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
