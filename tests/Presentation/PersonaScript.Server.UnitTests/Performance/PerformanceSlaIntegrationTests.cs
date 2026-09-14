using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Server.UnitTests.Auth;

namespace PersonaScript.Server.UnitTests.Performance;

public sealed class PerformanceSlaIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;
    private const double SlaMaxThresholdMs = 500.0;

    public PerformanceSlaIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Response_ShouldContainServerTimingHeader()
    {
        var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Server-Timing").Should().BeTrue("Server-Timing header must be emitted for performance observability");
        var timing = response.Headers.GetValues("Server-Timing").First();
        timing.Should().StartWith("app;dur=");
    }

    [Fact]
    public async Task HealthEndpoint_ShouldRespondUnderSlaThreshold()
    {
        var client = _factory.CreateClient();

        // Warmup
        await client.GetAsync("/health");

        var sw = Stopwatch.StartNew();
        using var response = await client.GetAsync("/health");
        sw.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.Elapsed.TotalMilliseconds.Should().BeLessThan(SlaMaxThresholdMs, "Health check must respond in < 500ms");
    }

    [Fact]
    public async Task StaticBlazorScript_ShouldRespondUnderSlaThreshold()
    {
        var client = _factory.CreateClient();

        // Warmup
        await client.GetAsync("/_framework/blazor.web.js");

        var sw = Stopwatch.StartNew();
        using var response = await client.GetAsync("/_framework/blazor.web.js");
        sw.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        sw.Elapsed.TotalMilliseconds.Should().BeLessThan(SlaMaxThresholdMs, "Static Blazor script must load in < 500ms");
    }

    [Fact]
    public async Task LoginAndCadastroPages_ShouldRespondUnderSlaThreshold()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Warmup
        await client.GetAsync("/login");

        var swLogin = Stopwatch.StartNew();
        using var loginResponse = await client.GetAsync("/login");
        swLogin.Stop();

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        swLogin.Elapsed.TotalMilliseconds.Should().BeLessThan(SlaMaxThresholdMs, "Login page must load in < 500ms");

        var swCadastro = Stopwatch.StartNew();
        using var cadastroResponse = await client.GetAsync("/cadastro");
        swCadastro.Stop();

        cadastroResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        swCadastro.Elapsed.TotalMilliseconds.Should().BeLessThan(SlaMaxThresholdMs, "Cadastro page must load in < 500ms");
    }

    [Fact]
    public async Task CachedPromptTemplateRepository_ShouldServeFromMemoryUnderTenMilliseconds()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPromptTemplateRepository>();

        var agentName = "SlaTestAgent_" + Guid.NewGuid().ToString("N");
        var template = PromptTemplate.Create(
            agentName,
            1,
            "System SLA prompt",
            "User SLA prompt",
            "{}",
            "SLA test",
            "admin@test.com",
            true).Value;

        await repo.AddAsync(template);

        // 1st call populates the cache
        var first = await repo.GetActiveByAgentNameAsync(agentName);
        first.Should().NotBeNull();

        // 2nd call served directly from memory cache
        var sw = Stopwatch.StartNew();
        var cached = await repo.GetActiveByAgentNameAsync(agentName);
        sw.Stop();

        cached.Should().NotBeNull();
        cached!.Id.Should().Be(template.Id);
        sw.Elapsed.TotalMilliseconds.Should().BeLessThan(15.0, "Serving active prompts from cache must take < 15ms");
    }
}
