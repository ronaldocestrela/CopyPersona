using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using PersonaScript.Server.Endpoints;
using PersonaScript.Server.UnitTests.Auth;

namespace PersonaScript.Server.UnitTests.Billing;

public class StripeEndpointsTests : IClassFixture<PersonaScriptWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StripeEndpointsTests(PersonaScriptWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task WebhookStripe_WithSampleJson_ShouldReturnOkStatus()
    {
        // Arrange
        var sampleJson = """
        {
          "id": "evt_test_webhook_123",
          "type": "customer.subscription.updated",
          "data": {
            "object": {
              "id": "sub_test_123",
              "customer": "cus_test_123",
              "current_period_start": 1700000000,
              "current_period_end": 1702500000
            }
          }
        }
        """;

        var content = new StringContent(sampleJson, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/webhooks/stripe", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
