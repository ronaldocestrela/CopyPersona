using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PersonaScript.Server.UnitTests.Auth;
using Xunit;

namespace PersonaScript.Server.UnitTests.Security;

public class RateLimitingIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public RateLimitingIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthEndpoint_WhenExceedingRateLimit_ShouldReturn429TooManyRequests()
    {
        // Arrange
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Security:RateLimiting:AuthPermitLimit", "2");
            builder.UseSetting("Security:RateLimiting:WindowSeconds", "60");
        });

        var client = customFactory.CreateClient();
        var body = new { Email = "test@example.com", Password = "WrongPassword123!" };

        // Act - requisições até o limite de 2
        var res1 = await client.PostAsJsonAsync("/account/token", body);
        var res2 = await client.PostAsJsonAsync("/account/token", body);

        // A 3ª requisição deve estourar o limite de 2 por janela
        var res3 = await client.PostAsJsonAsync("/account/token", body);

        // Assert
        res3.StatusCode.Should().Be((HttpStatusCode)429);
        res3.Headers.Should().ContainKey("Retry-After");
    }
}
