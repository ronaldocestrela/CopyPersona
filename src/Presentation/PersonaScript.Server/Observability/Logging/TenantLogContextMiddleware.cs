using Microsoft.AspNetCore.Http;
using PersonaScript.BuildingBlocks.Tenancy;
using Serilog.Context;

namespace PersonaScript.Server.Observability.Logging;

public sealed class TenantLogContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantLogContextMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = tenantContext.TenantId.Value != Guid.Empty ? tenantContext.TenantId.Value.ToString() : "anonymous";
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var traceId = context.TraceIdentifier;

        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            await _next(context);
        }
    }
}
