using System.Net;
using FluentAssertions;
using PersonaScript.Server.UnitTests.Auth;
using Xunit;

namespace PersonaScript.Server.UnitTests.Security;

public class SecurityHeadersIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public SecurityHeadersIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/health")]
    [InlineData("/login")]
    [InlineData("/cadastro")]
    public async Task Get_ShouldReturnOWASPRecommendedSecurityHeaders(string endpoint)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(endpoint);

        // Assert
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");

        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");

        response.Headers.Should().ContainKey("Referrer-Policy");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");

        response.Headers.Should().ContainKey("Permissions-Policy");
        var permPolicy = string.Join(";", response.Headers.GetValues("Permissions-Policy"));
        permPolicy.Should().Contain("camera=()");
        permPolicy.Should().Contain("microphone=()");

        response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = string.Join(";", response.Headers.GetValues("Content-Security-Policy"));
        csp.Should().Contain("default-src 'self'");
        csp.Should().Contain("frame-ancestors 'self'");
    }
}
