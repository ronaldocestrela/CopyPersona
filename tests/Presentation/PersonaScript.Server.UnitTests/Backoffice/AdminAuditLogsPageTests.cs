using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.GetAuditLogs;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminAuditLogsPageTests : BunitContext
{
    private readonly IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>> _getAuditLogsHandler = Substitute.For<IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>>();

    public AdminAuditLogsPageTests()
    {
        Services.AddSingleton(_getAuditLogsHandler);
    }

    [Fact]
    public void AuditLogsPage_WhenNoLogs_ShouldDisplayEmptyState()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetPolicies("RequireBackofficeAccess");

        _getAuditLogsHandler.Handle(Arg.Any<GetAuditLogsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<AuditLogDto>>(new List<AuditLogDto>())));

        // Act
        var cut = Render<AdminAuditLogsPage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Logs de Auditoria");
        cut.Find(".space-y-6").TextContent.Should().Contain("Nenhum registro de auditoria encetado");
    }

    [Fact]
    public void AuditLogsPage_WhenLogsExist_ShouldRenderTableWithActionAndAdminEmail()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetPolicies("RequireBackofficeAccess");

        var logs = new List<AuditLogDto>
        {
            new(
                Id: Guid.NewGuid(),
                ActionType: "ImpersonationStarted",
                AdminUserId: Guid.NewGuid(),
                AdminEmail: "support.agent@personascript.ai",
                TargetTenantId: Guid.NewGuid(),
                TargetUserEmail: "dr.ana@example.com",
                DetailsJson: "{\"Reason\": \"Suporte técnico no Wizard\"}",
                Timestamp: DateTimeOffset.UtcNow.AddMinutes(-10)
            )
        };

        _getAuditLogsHandler.Handle(Arg.Any<GetAuditLogsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<AuditLogDto>>(logs)));

        // Act
        var cut = Render<AdminAuditLogsPage>();

        // Assert
        cut.Find("tbody").TextContent.Should().Contain("ImpersonationStarted");
        cut.Find("tbody").TextContent.Should().Contain("support.agent@personascript.ai");
        cut.Find("tbody").TextContent.Should().Contain("dr.ana@example.com");
    }
}
