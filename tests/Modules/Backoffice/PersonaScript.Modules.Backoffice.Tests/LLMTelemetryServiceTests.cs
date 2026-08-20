using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PersonaScript.Modules.Backoffice.Application.Services;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Backoffice.Tests;

public class LLMTelemetryServiceTests
{
    [Fact]
    public async Task RecordExecutionAsync_ShouldPersistLogCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<BackofficeDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<Domain.Repositories.IAgentExecutionLogRepository, AgentExecutionLogRepository>();
        var serviceProvider = services.BuildServiceProvider();

        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var costCalculator = new LLMCostCalculator();
        var logger = NullLogger<LLMTelemetryService>.Instance;

        var telemetryService = new LLMTelemetryService(scopeFactory, costCalculator, logger);

        var tenantId = Guid.NewGuid();
        await telemetryService.RecordExecutionAsync(
            tenantId,
            "EstrategistaPersona",
            "gpt-4o",
            "OpenAI",
            1000,
            500,
            1200,
            AgentExecutionStatus.Success);

        using var verifyScope = serviceProvider.CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<BackofficeDbContext>();

        var persisted = await dbContext.AgentExecutionLogs.FirstOrDefaultAsync();
        Assert.NotNull(persisted);
        Assert.Equal(tenantId, persisted.TenantId);
        Assert.Equal("EstrategistaPersona", persisted.AgentName);
        Assert.Equal("gpt-4o", persisted.ModelUsed);
        Assert.Equal(1000, persisted.PromptTokens);
        Assert.Equal(500, persisted.CompletionTokens);
        Assert.Equal(1500, persisted.TotalTokens);
        Assert.Equal(0.0075m, persisted.EstimatedCostUSD); // (1000 * 2.5/1M) + (500 * 10/1M) = 0.0025 + 0.0050 = 0.0075
        Assert.Equal(AgentExecutionStatus.Success, persisted.Status);
    }
}
