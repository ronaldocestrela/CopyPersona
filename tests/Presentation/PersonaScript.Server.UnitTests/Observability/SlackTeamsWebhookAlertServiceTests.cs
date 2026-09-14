using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using PersonaScript.Server.Observability.Alerts;

namespace PersonaScript.Server.UnitTests.Observability;

public sealed class SlackTeamsWebhookAlertServiceTests
{
    private readonly ILogger<SlackTeamsWebhookAlertService> _logger = Substitute.For<ILogger<SlackTeamsWebhookAlertService>>();

    [Fact]
    public async Task NotifyPaymentWebhookFailureAsync_WhenDisabled_ShouldReturnFalseWithoutSendingHttp()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new AlertOptions { Enabled = false, WebhookUrl = "https://hooks.slack.com/test" });
        var service = new SlackTeamsWebhookAlertService(httpClient, options, _logger);

        var alert = new PaymentWebhookFailureAlert(
            EventType: "invoice.payment_failed",
            CustomerId: "cus_12345",
            ErrorCode: "card_declined",
            ErrorMessage: "O cartão de crédito foi recusado.",
            TimestampUtc: DateTime.UtcNow);

        // Act
        var result = await service.NotifyPaymentWebhookFailureAsync(alert);

        // Assert
        result.Should().BeFalse();
        handler.SendCount.Should().Be(0);
    }

    [Fact]
    public async Task NotifyPaymentWebhookFailureAsync_WhenSlackConfigured_ShouldPostSlackFormattedPayload()
    {
        // Arrange
        string? capturedPayload = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedPayload = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new AlertOptions
        {
            Enabled = true,
            WebhookUrl = "https://hooks.slack.com/services/T00/B00/XXXX",
            ChannelType = AlertChannelType.Slack,
            EnvironmentName = "Production"
        });
        var service = new SlackTeamsWebhookAlertService(httpClient, options, _logger);

        var alert = new PaymentWebhookFailureAlert(
            EventType: "invoice.payment_failed",
            CustomerId: "cus_998877",
            ErrorCode: "charge_failed",
            ErrorMessage: "Saldo insuficiente.",
            TimestampUtc: DateTime.UtcNow);

        // Act
        var result = await service.NotifyPaymentWebhookFailureAsync(alert);

        // Assert
        result.Should().BeTrue();
        handler.SendCount.Should().Be(1);
        capturedPayload.Should().NotBeNullOrWhiteSpace();
        capturedPayload.Should().Contain("invoice.payment_failed");
        capturedPayload.Should().Contain("cus_998877");
        capturedPayload.Should().Contain("charge_failed");
        capturedPayload.Should().Contain("Production");
    }

    [Fact]
    public async Task NotifyLLMQuotaOrFailureAsync_WhenTeamsConfigured_ShouldPostTeamsMessageCard()
    {
        // Arrange
        string? capturedPayload = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedPayload = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new AlertOptions
        {
            Enabled = true,
            WebhookUrl = "https://outlook.office.com/webhook/XXXX",
            ChannelType = AlertChannelType.Teams,
            EnvironmentName = "Staging"
        });
        var service = new SlackTeamsWebhookAlertService(httpClient, options, _logger);

        var alert = new LLMFailureAlert(
            ProviderType: "OpenAI",
            Model: "gpt-4o",
            ErrorCode: "RateLimitExceeded",
            ErrorMessage: "Quota de tokens esgotada no provedor.",
            LatencyMs: 1250,
            AllProvidersFailed: true,
            TimestampUtc: DateTime.UtcNow);

        // Act
        var result = await service.NotifyLLMQuotaOrFailureAsync(alert);

        // Assert
        result.Should().BeTrue();
        handler.SendCount.Should().Be(1);
        capturedPayload.Should().NotBeNullOrWhiteSpace();
        capturedPayload.Should().Contain("OpenAI");
        capturedPayload.Should().Contain("RateLimitExceeded");
        capturedPayload.Should().Contain("Quota de tokens esgotada");
        capturedPayload.Should().Contain("Staging");
    }

    [Fact]
    public async Task Notify_WhenHttpFailsWith500_ShouldReturnFalseWithoutThrowing()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var httpClient = new HttpClient(handler);
        var options = Options.Create(new AlertOptions
        {
            Enabled = true,
            WebhookUrl = "https://hooks.slack.com/services/T00/B00/FAIL",
            ChannelType = AlertChannelType.Slack
        });
        var service = new SlackTeamsWebhookAlertService(httpClient, options, _logger);

        var alert = new SystemCriticalAlert(
            Component: "Database",
            Title: "SQL Timeout",
            Message: "Conexão com SQL Server expirou.",
            StackTrace: null,
            TimestampUtc: DateTime.UtcNow);

        // Act
        var result = await service.NotifySystemCriticalErrorAsync(alert);

        // Assert
        result.Should().BeFalse();
        handler.SendCount.Should().Be(1);
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;
        public int SendCount { get; private set; }

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendCount++;
            return Task.FromResult(_handlerFunc(request));
        }
    }
}
