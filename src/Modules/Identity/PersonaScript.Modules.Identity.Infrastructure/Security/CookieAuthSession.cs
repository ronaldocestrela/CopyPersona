using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Identity.Application.Abstractions;

namespace PersonaScript.Modules.Identity.Infrastructure.Security;

public sealed class CookieAuthSession(IHttpContextAccessor httpContextAccessor) : IAuthSession
{
    public async Task SignInAsync(AuthUser user, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("role", user.Role),
            new(HttpContextTenantContext.TenantIdClaimType, user.UserId.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
            });
    }

    public Task SignOutAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext is null
            ? Task.CompletedTask
            : httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
