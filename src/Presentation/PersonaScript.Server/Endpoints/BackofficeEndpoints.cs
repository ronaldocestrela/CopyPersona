using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Backoffice.Application.Commands.FreezeAccount;
using PersonaScript.Modules.Backoffice.Application.Commands.GrantExtraCredits;
using PersonaScript.Modules.Backoffice.Application.Commands.ResetPassword;

namespace PersonaScript.Server.Endpoints;

public static class BackofficeEndpoints
{
    public static IEndpointRouteBuilder MapBackofficeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/backoffice");

        group.MapGet("/dashboard", () =>
            Results.Ok(new { status = "success", message = "Painel do Backoffice Operacional" }))
            .RequireAuthorization("RequireBackofficeAccess");

        group.MapGet("/admin-only", () =>
            Results.Ok(new { status = "success", message = "Área exclusiva de SystemAdmin" }))
            .RequireAuthorization("RequireSystemAdmin");

        group.MapGet("/support-only", () =>
            Results.Ok(new { status = "success", message = "Área de Suporte" }))
            .RequireAuthorization("RequireSupportAgent");

        group.MapGet("/finance-only", () =>
            Results.Ok(new { status = "success", message = "Área Financeira" }))
            .RequireAuthorization("RequireFinanceAdmin");

        group.MapPost("/impersonate/start", async (
            HttpContext context,
            IImpersonationService impersonationService,
            ImpersonateRequest request) =>
        {
            var adminIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@personascript.ai";

            if (!Guid.TryParse(adminIdClaim, out var adminUserId))
            {
                adminUserId = Guid.NewGuid();
            }

            var result = await impersonationService.StartImpersonationAsync(
                adminUserId,
                adminEmailClaim,
                request.TargetTenantId,
                request.TargetUserEmail,
                request.Reason,
                context.RequestAborted);

            if (result.IsFailure)
            {
                return Results.BadRequest(new { error = result.Error.Message });
            }

            return Results.Ok(new { success = true, redirectUrl = "/dashboard" });
        }).RequireAuthorization("RequireSupportAgent");

        group.MapPost("/impersonate/stop", async (
            HttpContext context,
            IImpersonationService impersonationService) =>
        {
            var result = await impersonationService.StopImpersonationAsync(context.RequestAborted);
            if (result.IsFailure)
            {
                return Results.BadRequest(new { error = result.Error.Message });
            }

            return Results.Ok(new { success = true, redirectUrl = "/admin/tenants" });
        }).RequireAuthorization("RequireBackofficeAccess");

        group.MapPost("/tenants/freeze", async (
            HttpContext context,
            ICommandHandler<FreezeTenantAccountCommand> handler,
            FreezeAccountRequest request) =>
        {
            var adminIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@personascript.ai";
            Guid.TryParse(adminIdClaim, out var adminUserId);

            var result = await handler.Handle(
                new FreezeTenantAccountCommand(adminUserId, adminEmailClaim, request.TargetTenantId, request.Reason),
                context.RequestAborted);

            return result.IsSuccess ? Results.Ok(new { success = true }) : Results.BadRequest(new { error = result.Error.Message });
        }).RequireAuthorization("RequireSupportAgent");

        group.MapPost("/tenants/unfreeze", async (
            HttpContext context,
            ICommandHandler<UnfreezeTenantAccountCommand> handler,
            UnfreezeAccountRequest request) =>
        {
            var adminIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@personascript.ai";
            Guid.TryParse(adminIdClaim, out var adminUserId);

            var result = await handler.Handle(
                new UnfreezeTenantAccountCommand(adminUserId, adminEmailClaim, request.TargetTenantId),
                context.RequestAborted);

            return result.IsSuccess ? Results.Ok(new { success = true }) : Results.BadRequest(new { error = result.Error.Message });
        }).RequireAuthorization("RequireSupportAgent");

        group.MapPost("/tenants/reset-password", async (
            HttpContext context,
            ICommandHandler<AdminResetUserPasswordCommand> handler,
            AdminResetPasswordRequest request) =>
        {
            var adminIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@personascript.ai";
            Guid.TryParse(adminIdClaim, out var adminUserId);

            var result = await handler.Handle(
                new AdminResetUserPasswordCommand(adminUserId, adminEmailClaim, request.TargetTenantId, request.NewPassword),
                context.RequestAborted);

            return result.IsSuccess ? Results.Ok(new { success = true }) : Results.BadRequest(new { error = result.Error.Message });
        }).RequireAuthorization("RequireSupportAgent");

        group.MapPost("/tenants/grant-credits", async (
            HttpContext context,
            ICommandHandler<GrantTenantExtraCreditsCommand> handler,
            GrantCreditsRequest request) =>
        {
            var adminIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var adminEmailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "admin@personascript.ai";
            Guid.TryParse(adminIdClaim, out var adminUserId);

            var result = await handler.Handle(
                new GrantTenantExtraCreditsCommand(adminUserId, adminEmailClaim, request.TargetTenantId, request.ExtraScripts, request.ExtraAiAnalyses, request.Reason),
                context.RequestAborted);

            return result.IsSuccess ? Results.Ok(new { success = true }) : Results.BadRequest(new { error = result.Error.Message });
        }).RequireAuthorization("RequireSupportAgent");

        return endpoints;
    }
}

public record ImpersonateRequest(Guid TargetTenantId, string TargetUserEmail, string Reason);
public record FreezeAccountRequest(Guid TargetTenantId, string Reason);
public record UnfreezeAccountRequest(Guid TargetTenantId);
public record AdminResetPasswordRequest(Guid TargetTenantId, string NewPassword);
public record GrantCreditsRequest(Guid TargetTenantId, int ExtraScripts, int ExtraAiAnalyses, string Reason);

