using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PersonaScript.Server.UnitTests.Auth;

namespace PersonaScript.Server.UnitTests.Observability;

public sealed class HealthCheckEndpointsTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(PersonaScriptWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetHealthLive_ShouldReturnOkWithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetHealthReady_ShouldReturnOkWithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task GetFullHealth_ShouldReturnJsonWithEntriesAndDuration()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("status", out var statusProp).Should().BeTrue();
        statusProp.GetString().Should().Be("Healthy");

        root.TryGetProperty("timestamp", out _).Should().BeTrue();
        root.TryGetProperty("totalDuration", out _).Should().BeTrue();
        root.TryGetProperty("entries", out var entriesProp).Should().BeTrue();

        // Deve conter verificações dos DbContexts
        entriesProp.TryGetProperty("IdentityDbContext", out _).Should().BeTrue();
        entriesProp.TryGetProperty("BillingDbContext", out _).Should().BeTrue();
    }
}
