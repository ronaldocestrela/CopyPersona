using FluentAssertions;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Tests.Domain;

public class ProcessedStripeEventTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccess()
    {
        // Act
        var result = ProcessedStripeEvent.Create("evt_12345", "customer.subscription.updated", Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("evt_12345");
        result.Value.EventType.Should().Be("customer.subscription.updated");
        result.Value.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithEmptyEventId_ShouldReturnValidationFailure()
    {
        // Act
        var result = ProcessedStripeEvent.Create("", "customer.subscription.updated");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ProcessedStripeEvent.EventIdRequired");
    }
}
