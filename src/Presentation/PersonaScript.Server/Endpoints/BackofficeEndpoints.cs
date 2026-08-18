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

        return endpoints;
    }
}
