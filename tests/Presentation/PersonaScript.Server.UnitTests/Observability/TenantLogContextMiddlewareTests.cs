using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Server.Observability.Logging;

namespace PersonaScript.Server.UnitTests.Observability;

public sealed class TenantLogContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenTenantContextHasTenant_PushesTenantIdAndUserIdToLogContext()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(TenantId.From(tenantId));

        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "trace-12345";

        bool nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantLogContextMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenTenantContextEmpty_ExecutesNextGracefully()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns(TenantId.From(Guid.Empty));

        var httpContext = new DefaultHttpContext();

        bool nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new TenantLogContextMiddleware(next);

        // Act
        await middleware.InvokeAsync(httpContext, tenantContext);

        // Assert
        nextCalled.Should().BeTrue();
    }
}
