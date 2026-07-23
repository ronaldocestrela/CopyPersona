using FluentAssertions;
using Microsoft.AspNetCore.Http;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class HttpContextTenantContextTests
{
    [Fact]
    public void TenantId_ShouldReturnEmpty_WhenClaimMissing()
    {
        var accessor = new HttpContextAccessor();
        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(Guid.Empty);
    }
}
