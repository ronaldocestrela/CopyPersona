using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Billing.Application.Commands.CreateCheckoutSession;
using PersonaScript.Modules.Billing.Application.Commands.CreateCustomerPortalSession;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices;
using PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Server.Components.Pages.MinhaConta;
using Xunit;

namespace PersonaScript.Server.UnitTests.Billing;

public class AssinaturaPageTests : BunitContext
{
    private readonly GetSubscriptionDetailsQueryHandler _subDetailsHandler;
    private readonly GetBillingInvoicesQueryHandler _invoicesHandler;
    private readonly CreateCustomerPortalSessionCommandHandler _portalHandler;
    private readonly CreateCheckoutSessionCommandHandler _checkoutHandler;

    public AssinaturaPageTests()
    {
        _subDetailsHandler = Substitute.ForPartsOf<GetSubscriptionDetailsQueryHandler>(null!, null!, null!, null!);
        _invoicesHandler = Substitute.ForPartsOf<GetBillingInvoicesQueryHandler>(null!, null!, null!);
        _portalHandler = Substitute.ForPartsOf<CreateCustomerPortalSessionCommandHandler>(null!, null!, null!);

        _checkoutHandler = Substitute.ForPartsOf<CreateCheckoutSessionCommandHandler>(null!, null!, null!);

        Services.AddSingleton(_subDetailsHandler);
        Services.AddSingleton(_invoicesHandler);
        Services.AddSingleton(_portalHandler);
        Services.AddSingleton(_checkoutHandler);
    }

    [Fact]
    public void AssinaturaPage_ShouldRenderSubscriptionDetailsAndQuotas()
    {
        // Arrange
        var availablePlans = new List<PlanDto>
        {
            new(Guid.NewGuid(), PlanType.Basic, "Plano Básico", "Básico", 0m, 0m, 1, 10, 5),
            new(Guid.NewGuid(), PlanType.Pro, "Plano Pro", "Pro", 99m, 990m, 3, 30, 15)
        };

        var details = new SubscriptionDetailsDto(
            SubscriptionId: Guid.NewGuid(),
            PlanId: availablePlans[1].Id,
            PlanType: PlanType.Pro,
            PlanName: "Plano Pro",
            MonthlyPrice: 99m,
            Status: SubscriptionStatus.Active,
            CurrentPeriodStart: DateTime.UtcNow.AddDays(-10),
            CurrentPeriodEnd: DateTime.UtcNow.AddDays(20),
            CancelAtPeriodEnd: false,
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            ScriptsGeneratedCount: 12,
            ScriptsLimit: 30,
            ActivePersonasCount: 2,
            ActivePersonasLimit: 3,
            AiAnalysesCount: 5,
            AiAnalysesLimit: 15,
            LastQuotaResetAt: DateTime.UtcNow.AddDays(-10),
            AvailablePlans: availablePlans);

        _subDetailsHandler.Handle(Arg.Any<GetSubscriptionDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(details));

        _invoicesHandler.Handle(Arg.Any<GetBillingInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new List<InvoiceDto>
            {
                new("inv_001", 99.00m, "BRL", "paid", "https://stripe.com/inv.pdf", DateTime.UtcNow.AddDays(-10))
            }));

        // Act
        var cut = Render<AssinaturaPage>();

        // Assert
        cut.Find(".current-plan-card").TextContent.Should().Contain("Plano Pro");
        cut.Find(".quota-usage-card").TextContent.Should().Contain("12 / 30"); // Franquia de roteiros
        cut.Find(".quota-usage-card").TextContent.Should().Contain("2 / 3");   // Personas
        cut.Find(".invoices-section").TextContent.Should().Contain("inv_001");  // Fatura
        cut.Find(".btn-portal").TextContent.Should().Contain("Gerenciar Assinatura & Faturas");


    }

    [Fact]
    public void AssinaturaPage_ShouldDisplayWarning_WhenCancelAtPeriodEndIsTrue()
    {
        // Arrange
        var details = new SubscriptionDetailsDto(
            SubscriptionId: Guid.NewGuid(),
            PlanId: Guid.NewGuid(),
            PlanType: PlanType.Pro,
            PlanName: "Plano Pro",
            MonthlyPrice: 99m,
            Status: SubscriptionStatus.Active,
            CurrentPeriodStart: DateTime.UtcNow.AddDays(-10),
            CurrentPeriodEnd: DateTime.UtcNow.AddDays(20),
            CancelAtPeriodEnd: true,
            StripeCustomerId: "cus_123",
            StripeSubscriptionId: "sub_123",
            ScriptsGeneratedCount: 5,
            ScriptsLimit: 30,
            ActivePersonasCount: 1,
            ActivePersonasLimit: 3,
            AiAnalysesCount: 2,
            AiAnalysesLimit: 15,
            LastQuotaResetAt: DateTime.UtcNow.AddDays(-10),
            AvailablePlans: new List<PlanDto>());

        _subDetailsHandler.Handle(Arg.Any<GetSubscriptionDetailsQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(details));

        _invoicesHandler.Handle(Arg.Any<GetBillingInvoicesQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new List<InvoiceDto>()));

        // Act
        var cut = Render<AssinaturaPage>();

        // Assert
        cut.Markup.Should().Contain("Sua assinatura será cancelada");
    }
}

