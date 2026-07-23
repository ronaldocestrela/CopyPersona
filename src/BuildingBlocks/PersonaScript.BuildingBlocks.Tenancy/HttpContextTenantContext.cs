using Microsoft.AspNetCore.Http;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.Tenancy;

public sealed class HttpContextTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public const string TenantIdClaimType = "tenant_id";

    public TenantId TenantId
    {
        get
        {
            var claimValue = httpContextAccessor.HttpContext?.User?.FindFirst(TenantIdClaimType)?.Value;
            return Guid.TryParse(claimValue, out var tenantId)
                ? TenantId.From(tenantId)
                : TenantId.From(Guid.Empty);
        }
    }
}
