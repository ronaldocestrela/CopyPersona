using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Server.UnitTests.Auth;

public class BackofficeAuthorizationIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public BackofficeAuthorizationIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task BackofficeDashboard_ShouldReturn401_WhenUnauthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/backoffice/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BackofficeDashboard_ShouldReturn403_WhenUserIsSubscriber()
    {
        var client = _factory.CreateClient();
        var token = GenerateTokenForRole(UserRole.Subscriber);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/backoffice/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task BackofficeDashboard_ShouldReturn200_WhenUserIsSupportAgent()
    {
        var client = _factory.CreateClient();
        var token = GenerateTokenForRole(UserRole.SupportAgent);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/backoffice/dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_ShouldReturn403_WhenUserIsSupportAgent()
    {
        var client = _factory.CreateClient();
        var token = GenerateTokenForRole(UserRole.SupportAgent);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/backoffice/admin-only");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminOnlyEndpoint_ShouldReturn200_WhenUserIsSystemAdmin()
    {
        var client = _factory.CreateClient();
        var token = GenerateTokenForRole(UserRole.SystemAdmin);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/backoffice/admin-only");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private string GenerateTokenForRole(UserRole role)
    {
        using var scope = _factory.Services.CreateScope();
        var generator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var user = User.Register($"User {role}", $"{role.ToString().ToLower()}@example.com", "hash_password").Value;
        user.AssignRole(role);

        var result = generator.GenerateToken(user);
        return result.AccessToken;
    }
}
