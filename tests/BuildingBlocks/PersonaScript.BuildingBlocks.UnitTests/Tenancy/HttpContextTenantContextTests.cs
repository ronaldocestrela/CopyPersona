using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class HttpContextTenantContextTests
{
    [Fact]
    public void TenantId_ShouldReturnEmpty_WhenHttpContextIsNull()
    {
        var accessor = new HttpContextAccessor();
        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantId_ShouldReturnEmpty_WhenUserNotAuthenticated()
    {
        var expectedGuid = Guid.NewGuid();
        var claims = new[] { new Claim(HttpContextTenantContext.TenantIdClaimType, expectedGuid.ToString()) };
        var identity = new ClaimsIdentity(claims); // IsAuthenticated == false (no auth type)
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantId_ShouldReturnEmpty_WhenClaimMissing()
    {
        var identity = new ClaimsIdentity(Array.Empty<Claim>(), "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TenantId_ShouldReturnTenantId_WhenTenantIdClaimPresent()
    {
        var expectedGuid = Guid.NewGuid();
        var claims = new[] { new Claim(HttpContextTenantContext.TenantIdClaimType, expectedGuid.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void TenantId_ShouldFallbackToNameIdentifier_WhenTenantIdClaimMissing()
    {
        var expectedGuid = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, expectedGuid.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void TenantId_ShouldFallbackToSub_WhenTenantIdAndNameIdentifierMissing()
    {
        var expectedGuid = Guid.NewGuid();
        var claims = new[] { new Claim("sub", expectedGuid.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(expectedGuid);
    }

    [Fact]
    public void TenantId_ShouldReturnEmpty_WhenClaimValueIsNotValidGuid()
    {
        var claims = new[] { new Claim(HttpContextTenantContext.TenantIdClaimType, "invalid-guid-string") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        var context = new HttpContextTenantContext(accessor);

        context.TenantId.Value.Should().Be(Guid.Empty);
    }
}
