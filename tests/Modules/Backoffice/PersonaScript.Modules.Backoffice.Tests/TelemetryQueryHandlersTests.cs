using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Application.Queries.Telemetry;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Infrastructure.Repositories;
using Xunit;

namespace PersonaScript.Modules.Backoffice.Tests;

public class TelemetryQueryHandlersTests
{
    private BackofficeDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BackofficeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BackofficeDbContext(options);
    }

    [Fact]
    public async Task GetTelemetrySummaryQueryHandler_ShouldReturnAggregatedMetrics()
    {
        var db = CreateDbContext();
        var repo = new AgentExecutionLogRepository(db);

        var tenant1 = Guid.NewGuid();
        var log1 = AgentExecutionLog.Create(tenant1, "EstrategistaPersona", "gpt-4o", "OpenAI", 1000, 500, 0.0075m, 1000, AgentExecutionStatus.Success).Value;
        var log2 = AgentExecutionLog.Create(tenant1, "CopywriterVideo", "gpt-4o-mini", "OpenAI", 2000, 1000, 0.0009m, 800, AgentExecutionStatus.Success).Value;

        await db.AgentExecutionLogs.AddRangeAsync(log1, log2);
        await db.SaveChangesAsync();

        var handler = new GetTelemetrySummaryQueryHandler(repo);
        var query = new GetTelemetrySummaryQuery(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalExecutions);
        Assert.Equal(2, result.Value.SuccessfulExecutions);
        Assert.Equal(3000, result.Value.TotalPromptTokens);
        Assert.Equal(1500, result.Value.TotalCompletionTokens);
        Assert.Equal(4500, result.Value.TotalTokens);
        Assert.Equal(0.0084m, result.Value.TotalCostUSD);
    }

    [Fact]
    public async Task GetAnomalyAlertsQueryHandler_ShouldDetectHighTenantCostAndModelErrors()
    {
        var db = CreateDbContext();
        var repo = new AgentExecutionLogRepository(db);

        var highCostTenant = Guid.NewGuid();

        // 1. Add log with cost >= $10.00
        var highCostLog = AgentExecutionLog.Create(highCostTenant, "EstrategistaPersona", "gpt-4o", "OpenAI", 4000000, 1000000, 15.00m, 2000, AgentExecutionStatus.Success).Value;
        await db.AgentExecutionLogs.AddAsync(highCostLog);

        // 2. Add 10 logs for model with failures
        for (int i = 0; i < 10; i++)
        {
            var status = i < 3 ? AgentExecutionStatus.Failure : AgentExecutionStatus.Success; // 30% failure rate
            var modelLog = AgentExecutionLog.Create(Guid.NewGuid(), "TestAgent", "buggy-model", "TestProvider", 100, 100, 0.001m, 500, status, status == AgentExecutionStatus.Failure ? "Error" : null).Value;
            await db.AgentExecutionLogs.AddAsync(modelLog);
        }

        await db.SaveChangesAsync();

        var handler = new GetAnomalyAlertsQueryHandler(repo);
        var result = await handler.Handle(new GetAnomalyAlertsQuery(10.00m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value);
        Assert.Contains(result.Value, a => a.AlertType == "HighTenantCost" && a.TenantId == highCostTenant);
        Assert.Contains(result.Value, a => a.AlertType == "ModelErrorSpike");
    }
}
