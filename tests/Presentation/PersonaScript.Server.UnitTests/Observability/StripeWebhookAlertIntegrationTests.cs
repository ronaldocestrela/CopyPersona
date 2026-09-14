using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using PersonaScript.Server.Observability.Alerts;
using PersonaScript.Server.UnitTests.Auth;

namespace PersonaScript.Server.UnitTests.Observability;

public sealed class StripeWebhookAlertIntegrationTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly PersonaScriptWebApplicationFactory _factory;

    public StripeWebhookAlertIntegrationTests(PersonaScriptWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostWebhook_WithInvalidSignature_ShouldTriggerPaymentAlertAndReturnBadRequest()
    {
        // Arrange
        var alertService = Substitute.For<IOperationalAlertService>();
        alertService.NotifyPaymentWebhookFailureAsync(Arg.Any<PaymentWebhookFailureAlert>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped(_ => alertService);
            });
        }).CreateClient();

        var content = new StringContent("{\"type\":\"invoice.payment_failed\"}", Encoding.UTF8, "application/json");
        content.Headers.Add("Stripe-Signature", "t=12345,v1=invalidsignature");

        // Act
        var response = await client.PostAsync("/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        await alertService.Received(1).NotifyPaymentWebhookFailureAsync(
            Arg.Is<PaymentWebhookFailureAlert>(a => a != null && a.ErrorCode != null),
            Arg.Any<CancellationToken>());
    }
}
