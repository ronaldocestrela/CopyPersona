using FluentAssertions;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.BuildingBlocks.UnitTests.Domain;

public class DomainTests
{
    private sealed class SampleEntity : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; init; }
    }

    private sealed class Money : ValueObject
    {
        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; }
        public string Currency { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void BaseEntity_ShouldGenerateId()
    {
        var entity = new SampleEntity { TenantId = Guid.NewGuid() };

        entity.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ValueObject_ShouldCompareByComponents()
    {
        var left = new Money(10m, "BRL");
        var right = new Money(10m, "BRL");
        var different = new Money(20m, "BRL");

        left.Should().Be(right);
        left.Should().NotBe(different);
        left.GetHashCode().Should().Be(right.GetHashCode());
    }
}
