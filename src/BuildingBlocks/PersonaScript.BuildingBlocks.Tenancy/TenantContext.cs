namespace PersonaScript.BuildingBlocks.Tenancy;

public readonly record struct TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

public interface ITenantContext
{
    TenantId TenantId { get; }
}

public sealed class NullTenantContext : ITenantContext
{
    public TenantId TenantId { get; } = TenantId.From(Guid.Empty);
}

public sealed class FixedTenantContext(TenantId tenantId) : ITenantContext
{
    public TenantId TenantId { get; } = tenantId;
}
