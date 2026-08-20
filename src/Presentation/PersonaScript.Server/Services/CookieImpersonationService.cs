using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Backoffice.Application.Commands.Impersonation;

namespace PersonaScript.Server.Services;

public sealed class CookieImpersonationService(
    IHttpContextAccessor httpContextAccessor,
    ICommandHandler<StartImpersonationCommand, Guid> startHandler,
    ICommandHandler<StopImpersonationCommand> stopHandler) : IImpersonationService
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public bool IsImpersonating
    {
        get
        {
            var user = HttpContext?.User;
            return user?.HasClaim(c => c.Type == "is_impersonating" && c.Value == "true") ?? false;
        }
    }

    public Guid? TargetTenantId
    {
        get
        {
            var claimValue = HttpContext?.User?.FindFirst("impersonated_tenant_id")?.Value;
            return Guid.TryParse(claimValue, out var guid) ? guid : null;
        }
    }

    public string? TargetUserEmail => HttpContext?.User?.FindFirst("impersonated_user_email")?.Value;

    public async Task<Result> StartImpersonationAsync(
        Guid adminUserId,
        string adminEmail,
        Guid targetTenantId,
        string targetUserEmail,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (HttpContext is null)
        {
            return Result.Failure(Error.Failure("Impersonation.NoContext", "Contexto HTTP indisponível."));
        }

        var startResult = await startHandler.Handle(
            new StartImpersonationCommand(adminUserId, adminEmail, targetTenantId, reason),
            cancellationToken);

        if (startResult.IsFailure)
        {
            return Result.Failure(startResult.Error);
        }

        var user = HttpContext.User;
        var existingClaims = user.Claims.Where(c =>
            c.Type != "impersonated_tenant_id" &&
            c.Type != "impersonated_user_email" &&
            c.Type != "is_impersonating").ToList();

        existingClaims.Add(new Claim("impersonated_tenant_id", targetTenantId.ToString()));
        existingClaims.Add(new Claim("impersonated_user_email", targetUserEmail));
        existingClaims.Add(new Claim("is_impersonating", "true"));

        var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });

        return Result.Success();
    }

    public async Task<Result> StopImpersonationAsync(CancellationToken cancellationToken = default)
    {
        if (HttpContext is null)
        {
            return Result.Failure(Error.Failure("Impersonation.NoContext", "Contexto HTTP indisponível."));
        }

        var adminUserIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmailClaim = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        if (Guid.TryParse(adminUserIdClaim, out var adminUserId))
        {
            await stopHandler.Handle(new StopImpersonationCommand(adminUserId, adminEmailClaim), cancellationToken);
        }

        var existingClaims = HttpContext.User.Claims.Where(c =>
            c.Type != "impersonated_tenant_id" &&
            c.Type != "impersonated_user_email" &&
            c.Type != "is_impersonating").ToList();

        var identity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });

        return Result.Success();
    }
}
