using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PersonaScript.BuildingBlocks.Tenancy;

public sealed class HttpContextTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public const string TenantIdClaimType = "tenant_id";

    public TenantId TenantId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity is not { IsAuthenticated: true })
            {
                return TenantId.From(Guid.Empty);
            }

            var claimValue = user.FindFirst(TenantIdClaimType)?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            return Guid.TryParse(claimValue, out var tenantId)
                ? TenantId.From(tenantId)
                : TenantId.From(Guid.Empty);
        }
    }
}
