using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.Telemetry;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminTelemetryPageTests : BunitContext
{
    private readonly IQueryHandler<GetTelemetrySummaryQuery, TelemetrySummaryDto> _getTelemetrySummaryHandler = Substitute.For<IQueryHandler<GetTelemetrySummaryQuery, TelemetrySummaryDto>>();
    private readonly IQueryHandler<GetAgentExecutionLogsQuery, GetAgentExecutionLogsResult> _getAgentExecutionLogsHandler = Substitute.For<IQueryHandler<GetAgentExecutionLogsQuery, GetAgentExecutionLogsResult>>();
    private readonly IQueryHandler<GetAnomalyAlertsQuery, IReadOnlyList<AnomalyAlertDto>> _getAnomalyAlertsHandler = Substitute.For<IQueryHandler<GetAnomalyAlertsQuery, IReadOnlyList<AnomalyAlertDto>>>();

    public AdminTelemetryPageTests()
    {
        Services.AddSingleton(_getTelemetrySummaryHandler);
        Services.AddSingleton(_getAgentExecutionLogsHandler);
        Services.AddSingleton(_getAnomalyAlertsHandler);
    }

    [Fact]
    public void TelemetryPage_ShouldRenderMetricsAndAlerts()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetPolicies("RequireBackofficeAccess");

        var summary = new TelemetrySummaryDto
        {
            TotalExecutions = 250,
            SuccessfulExecutions = 245,
            FailedExecutions = 5,
            SuccessRatePercent = 98.0,
            TotalPromptTokens = 150000,
            TotalCompletionTokens = 50000,
            TotalCostUSD = 1.45m,
            TotalSubscriptionRevenueUSD = 2500m,
            LLMCostMarginPercent = 99.9,
            AverageLatencyMs = 1200
        };

        var alert = new AnomalyAlertDto
        {
            AlertType = "LatencySpike",
            Severity = "Critical",
            Title = "Pico de Latência Detectado",
            Description = "Latência média superou 5 segundos nos últimos 10 minutos",
            TenantId = null,
            DetectedAtUtc = DateTime.UtcNow
        };

        var log = new AgentExecutionLogDto
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            AgentName = "EstrategistaPersona",
            ModelUsed = "gemini-1.5-pro",
            ProviderType = "Gemini",
            PromptTokens = 800,
            CompletionTokens = 400,
            TotalTokens = 1200,
            EstimatedCostUSD = 0.003m,
            LatencyMs = 1100,
            Status = "Success",
            ErrorMessage = null,
            ExecutedAtUtc = DateTime.UtcNow
        };

        _getTelemetrySummaryHandler.Handle(Arg.Any<GetTelemetrySummaryQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(summary)));

        _getAnomalyAlertsHandler.Handle(Arg.Any<GetAnomalyAlertsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<AnomalyAlertDto>>(new List<AnomalyAlertDto> { alert })));

        _getAgentExecutionLogsHandler.Handle(Arg.Any<GetAgentExecutionLogsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new GetAgentExecutionLogsResult(new List<AgentExecutionLogDto> { log }, 1, 1, 25))));

        // Act
        var cut = Render<AdminTelemetryPage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Telemetria & Observabilidade de IA");
        cut.Find(".space-y-6").TextContent.Should().Contain("Pico de Latência Detectado");
        cut.Find(".space-y-6").TextContent.Should().Contain("EstrategistaPersona");
        cut.Find(".space-y-6").TextContent.Should().Contain("gemini-1.5-pro");
    }
}
