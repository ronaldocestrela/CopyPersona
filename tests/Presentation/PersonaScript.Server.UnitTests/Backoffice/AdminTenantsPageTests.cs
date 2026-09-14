using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.GetTenantDetails;
using PersonaScript.Modules.Backoffice.Application.Queries.GetTenants;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminTenantsPageTests : BunitContext
{
    private readonly IQueryHandler<GetTenantsQuery, GetTenantsResult> _getTenantsHandler = Substitute.For<IQueryHandler<GetTenantsQuery, GetTenantsResult>>();
    private readonly IQueryHandler<GetTenantDetailsQuery, TenantDetailsDto> _getTenantDetailsHandler = Substitute.For<IQueryHandler<GetTenantDetailsQuery, TenantDetailsDto>>();

    public AdminTenantsPageTests()
    {
        Services.AddSingleton(_getTenantsHandler);
        Services.AddSingleton(_getTenantDetailsHandler);
    }

    [Fact]
    public void TenantsPage_ShouldRenderTenantListAndMetrics()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("support@personascript.ai");
        authContext.SetPolicies("RequireSupportAgent");

        var tenantA = new TenantSummaryDto(
            TenantId: Guid.NewGuid(),
            FullName: "Dra. Maria Clara",
            Email: "maria@example.com",
            Role: "Subscriber",
            PlanName: "Plano Pro",
            SubscriptionStatus: "Active",
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-30),
            IsFrozen: false,
            FreezeReason: null,
            ScriptsGeneratedCount: 5,
            ScriptsLimit: 30,
            AiAnalysesCount: 1,
            AiAnalysesLimit: 3);

        var tenantB = new TenantSummaryDto(
            TenantId: Guid.NewGuid(),
            FullName: "Dr. Roberto Silva",
            Email: "roberto@example.com",
            Role: "Subscriber",
            PlanName: "Plano Basic",
            SubscriptionStatus: "Active",
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-15),
            IsFrozen: true,
            FreezeReason: "Chargeback em análise",
            ScriptsGeneratedCount: 10,
            ScriptsLimit: 10,
            AiAnalysesCount: 1,
            AiAnalysesLimit: 1);

        var tenantsResult = new GetTenantsResult(new List<TenantSummaryDto> { tenantA, tenantB }, 2, 1, 20);
        _getTenantsHandler.Handle(Arg.Any<GetTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(tenantsResult)));

        // Act
        var cut = Render<AdminTenantsPage>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Tenants & Usuários");
        cut.Find(".space-y-6").TextContent.Should().Contain("Dra. Maria Clara");
        cut.Find(".space-y-6").TextContent.Should().Contain("Dr. Roberto Silva");
        cut.Find(".space-y-6").TextContent.Should().Contain("Total Assinantes");
    }

    [Fact]
    public void TenantsPage_ClickingDetails_ShouldLoadAndDisplayTenantDetailsDrawer()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("support@personascript.ai");
        authContext.SetPolicies("RequireSupportAgent");

        var tenantA = new TenantSummaryDto(
            TenantId: Guid.NewGuid(),
            FullName: "Dra. Maria Clara",
            Email: "maria@example.com",
            Role: "Subscriber",
            PlanName: "Plano Pro",
            SubscriptionStatus: "Active",
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-30),
            IsFrozen: false,
            FreezeReason: null,
            ScriptsGeneratedCount: 5,
            ScriptsLimit: 30,
            AiAnalysesCount: 1,
            AiAnalysesLimit: 3);

        var tenantsResult = new GetTenantsResult(new List<TenantSummaryDto> { tenantA }, 1, 1, 20);
        _getTenantsHandler.Handle(Arg.Any<GetTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(tenantsResult)));

        var details = new TenantDetailsDto(
            Summary: tenantA,
            Anamnese: new AnamneseInfoDto("Dentista", "Ortodontia", "Invisalign", "Adultos 25-45", "Empático", "Captação", DateTime.UtcNow),
            DiagnosesCount: 1,
            ScriptsCount: 5,
            AuditHistory: new List<AuditLogDto>());

        _getTenantDetailsHandler.Handle(Arg.Any<GetTenantDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(details)));

        var cut = Render<AdminTenantsPage>();

        // Act - Click "Ver Detalhes"
        var detailsBtn = cut.Find("button[title='Ver Detalhes do Tenant']");
        detailsBtn.Click();

        // Assert
        _getTenantDetailsHandler.Received(1).Handle(Arg.Any<GetTenantDetailsQuery>(), Arg.Any<CancellationToken>());
        var modal = cut.Find(".fixed.inset-0");
        modal.TextContent.Should().Contain("Dra. Maria Clara");
        modal.TextContent.Should().Contain("Histórico de Auditoria do Tenant");
    }
}
