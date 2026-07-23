using FluentAssertions;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class TenancyTests
{
    [Fact]
    public void TenantId_ShouldWrapGuidValue()
    {
        var guid = Guid.NewGuid();
        var tenantId = TenantId.From(guid);

        tenantId.Value.Should().Be(guid);
        tenantId.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void FixedTenantContext_ShouldExposeTenantId()
    {
        var tenantId = TenantId.New();
        var context = new FixedTenantContext(tenantId);

        context.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void NullTenantContext_ShouldUseEmptyGuid()
    {
        var context = new NullTenantContext();

        context.TenantId.Value.Should().Be(Guid.Empty);
    }
}
