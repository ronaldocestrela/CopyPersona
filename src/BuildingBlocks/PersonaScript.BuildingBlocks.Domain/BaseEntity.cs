namespace PersonaScript.BuildingBlocks.Domain;

public interface IAggregateRoot;

public interface IMustHaveTenant
{
    Guid TenantId { get; }
}

public abstract class BaseEntity : IAggregateRoot
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(default(HashCode), (hash, component) =>
            {
                hash.Add(component);
                return hash;
            })
            .ToHashCode();
    }
}
