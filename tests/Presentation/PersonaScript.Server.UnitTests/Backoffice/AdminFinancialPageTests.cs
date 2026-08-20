using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota;
using PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans;
using PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics;
using PersonaScript.Modules.Backoffice.Application.Queries.GetTenants;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Server.Components.Pages.Admin;
using Xunit;

namespace PersonaScript.Server.UnitTests.Backoffice;

public class AdminFinancialPageTests : BunitContext
{
    private readonly IQueryHandler<GetFinancialMetricsQuery, FinancialMetricsDto> _getFinancialMetricsHandler = Substitute.For<IQueryHandler<GetFinancialMetricsQuery, FinancialMetricsDto>>();
    private readonly IQueryHandler<GetAllPlansQuery, IReadOnlyList<PlanDto>> _getAllPlansHandler = Substitute.For<IQueryHandler<GetAllPlansQuery, IReadOnlyList<PlanDto>>>();
    private readonly IQueryHandler<GetTenantsQuery, GetTenantsResult> _getTenantsHandler = Substitute.For<IQueryHandler<GetTenantsQuery, GetTenantsResult>>();
    private readonly ICommandHandler<UpdatePlanLimitsCommand> _updatePlanLimitsHandler = Substitute.For<ICommandHandler<UpdatePlanLimitsCommand>>();
    private readonly ICommandHandler<OverrideTenantQuotaCommand> _overrideTenantQuotaHandler = Substitute.For<ICommandHandler<OverrideTenantQuotaCommand>>();

    public AdminFinancialPageTests()
    {
        Services.AddSingleton(_getFinancialMetricsHandler);
        Services.AddSingleton(_getAllPlansHandler);
        Services.AddSingleton(_getTenantsHandler);
        Services.AddSingleton(_updatePlanLimitsHandler);
        Services.AddSingleton(_overrideTenantQuotaHandler);
    }

    [Fact]
    public void AdminFinancialPage_ShouldRenderHeaderAndKpiCards()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");
        authContext.SetClaims(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "FinanceAdmin"));

        var planSummary = new PlanFinancialSummaryDto(Guid.NewGuid(), "Pro", "Plano Pro", 97.00m, 5, 485.00m);
        var metrics = new FinancialMetricsDto(
            485.00m,
            5820.00m,
            5,
            5,
            0,
            0,
            0,
            0.0,
            0.00m,
            new List<PlanFinancialSummaryDto> { planSummary });

        _getFinancialMetricsHandler.Handle(Arg.Any<GetFinancialMetricsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(metrics)));

        _getAllPlansHandler.Handle(Arg.Any<GetAllPlansQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PlanDto>>(new List<PlanDto>())));

        _getTenantsHandler.Handle(Arg.Any<GetTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new GetTenantsResult(new List<TenantSummaryDto>(), 0, 1, 100))));

        // Act
        var cut = Render<AdminFinancialPage>();

        // Assert
        cut.Markup.Should().Contain("Gestão Financeira & Planos");
        cut.Markup.Should().Contain("MRR (Mensal)");
        cut.Markup.Should().Contain("485");
        cut.Markup.Should().Contain("ARR (Anual Projetado)");
        cut.Markup.Should().Contain("5");
        cut.Markup.Should().Contain("Taxa de Churn");
    }


    [Fact]
    public void AdminFinancialPage_TabSwitching_ShouldChangeActiveTab()
    {
        // Arrange
        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("admin@personascript.ai");

        var planDto = new PlanDto(Guid.NewGuid(), PlanType.Pro, "Plano Pro", "Descrição", 97m, 970m, 5, 30, 50, true, null);

        _getFinancialMetricsHandler.Handle(Arg.Any<GetFinancialMetricsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new FinancialMetricsDto(0, 0, 0, 0, 0, 0, 0, 0, 0, new List<PlanFinancialSummaryDto>()))));

        _getAllPlansHandler.Handle(Arg.Any<GetAllPlansQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<PlanDto>>(new List<PlanDto> { planDto })));

        _getTenantsHandler.Handle(Arg.Any<GetTenantsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(new GetTenantsResult(new List<TenantSummaryDto>(), 0, 1, 100))));


        // Act
        var cut = Render<AdminFinancialPage>();

        // Switch to Plans Tab
        var plansTabButton = cut.FindAll("button").First(b => b.TextContent.Contains("Gestão de Planos & Franquias"));
        plansTabButton.Click();

        // Assert
        cut.Markup.Should().Contain("Catálogo de Planos SaaS");
        cut.Markup.Should().Contain("Plano Pro");
    }
}
