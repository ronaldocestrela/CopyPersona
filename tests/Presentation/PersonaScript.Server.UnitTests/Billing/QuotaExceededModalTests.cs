using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.Server.Components.Billing;
using PersonaScript.Server.Services;

namespace PersonaScript.Server.UnitTests.Billing;

public class QuotaExceededModalTests : BunitContext
{
    private readonly QuotaNotifierService _quotaNotifier = new();

    public QuotaExceededModalTests()
    {
        Services.AddSingleton<IQuotaNotifierService>(_quotaNotifier);
    }

    [Fact]
    public void Modal_WhenDefault_ShouldNotBeVisible()
    {
        // Act
        var cut = Render<QuotaExceededModal>();

        // Assert
        cut.FindAll(".quota-modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenQuotaExceededEventFires_ShouldRenderTitleAndMessage()
    {
        // Arrange
        var cut = Render<QuotaExceededModal>();

        // Act
        cut.InvokeAsync(() => _quotaNotifier.NotifyQuotaExceeded("Roteiros de Vídeo", "Você atingiu o limite mensal de 10 roteiros no seu plano atual."));

        // Assert
        cut.Find(".quota-modal-title").TextContent.Should().Contain("Limite do Seu Plano Atingido");
        cut.Find(".quota-modal-body").TextContent.Should().Contain("Roteiros de Vídeo");
        cut.Find(".quota-modal-body").TextContent.Should().Contain("Você atingiu o limite mensal de 10 roteiros");
    }

    [Fact]
    public void Modal_WhenCloseClicked_ShouldHideModal()
    {
        // Arrange
        var cut = Render<QuotaExceededModal>();
        cut.InvokeAsync(() => _quotaNotifier.NotifyQuotaExceeded("Roteiros de Vídeo", "Limite atingido."));

        // Act
        cut.Find("button.btn-secondary").Click();

        // Assert
        cut.FindAll(".quota-modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenUpgradeClicked_ShouldNavigateToSubscriptionPage()
    {
        // Arrange
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = Render<QuotaExceededModal>();
        cut.InvokeAsync(() => _quotaNotifier.NotifyQuotaExceeded("Análises de IA", "Limite atingido."));

        // Act
        cut.Find("button.btn-primary").Click();

        // Assert
        nav.Uri.Should().EndWith("/minha-conta/assinatura");
    }
}

