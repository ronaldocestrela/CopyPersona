using FluentAssertions;
using PersonaScript.BuildingBlocks.Domain;

namespace PersonaScript.BuildingBlocks.UnitTests.Domain;

public class DomainTests
{
    private class TestEvent(string name) : DomainEvent
    {
        public string Name { get; } = name;
    }

    private class TestEntity : BaseEntity
    {
    }

    private class TestValueObject(string city, string country) : ValueObject
    {
        public string City { get; } = city;
        public string Country { get; } = country;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return City;
            yield return Country;
        }
    }

    [Fact]
    public void BaseEntity_ShouldGenerateGuidId_WhenInstantiated()
    {
        var entity = new TestEntity();

        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void BaseEntity_ShouldManageDomainEvents()
    {
        var entity = new TestEntity();
        var evt1 = new TestEvent("Event1");
        var evt2 = new TestEvent("Event2");

        entity.DomainEvents.Should().BeEmpty();

        entity.AddDomainEvent(evt1);
        entity.AddDomainEvent(evt2);

        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().Contain([evt1, evt2]);

        entity.RemoveDomainEvent(evt1);
        entity.DomainEvents.Should().HaveCount(1);
        entity.DomainEvents.Should().Contain(evt2);

        entity.ClearDomainEvents();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ValueObject_ShouldBeEqual_WhenComponentsAreEqual()
    {
        var vo1 = new TestValueObject("São Paulo", "Brasil");
        var vo2 = new TestValueObject("São Paulo", "Brasil");
        var vo3 = new TestValueObject("Rio de Janeiro", "Brasil");

        vo1.Equals(vo2).Should().BeTrue();
        (vo1 == vo2).Should().BeFalse(); // ValueObject equals method
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());

        vo1.Equals(vo3).Should().BeFalse();
    }
}
