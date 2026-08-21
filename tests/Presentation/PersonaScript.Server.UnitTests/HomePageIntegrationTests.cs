using System.Net;
using FluentAssertions;
using PersonaScript.Server.UnitTests.Auth;

namespace PersonaScript.Server.UnitTests;

public sealed class HomePageIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public HomePageIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Home_ShouldReturnOkAndReferenceBlazorWebScript()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        using var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("_framework/blazor.web.js");
    }

    [Fact]
    public async Task BlazorWebScript_ShouldReturnOkWithJavaScriptBody()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });

        using var response = await client.GetAsync("/_framework/blazor.web.js");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotBeNullOrWhiteSpace();
        body.Should().Contain("Blazor", "blazor.web.js must contain the Blazor runtime bootstrap");
    }
}
