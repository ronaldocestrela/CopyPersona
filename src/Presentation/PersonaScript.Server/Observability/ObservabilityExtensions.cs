using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;
using PersonaScript.Server.Observability.Alerts;
using PersonaScript.Server.Observability.Health;
using PersonaScript.Server.Observability.Logging;

namespace PersonaScript.Server.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddPersonaScriptObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AlertOptions>(configuration.GetSection(AlertOptions.SectionName));

        services.AddHttpClient<IOperationalAlertService, SlackTeamsWebhookAlertService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddSingleton<ILLMFailureNotifier, OperationalLLMFailureNotifier>();

        services.AddHealthChecks()
            .AddDbContextCheck<IdentityDbContext>("IdentityDbContext", tags: new[] { "ready", "db" })
            .AddDbContextCheck<BillingDbContext>("BillingDbContext", tags: new[] { "ready", "db" })
            .AddDbContextCheck<AnamneseDbContext>("AnamneseDbContext", tags: new[] { "ready", "db" })
            .AddDbContextCheck<PersonasDbContext>("PersonasDbContext", tags: new[] { "ready", "db" })
            .AddDbContextCheck<ScriptsDbContext>("ScriptsDbContext", tags: new[] { "ready", "db" })
            .AddDbContextCheck<BackofficeDbContext>("BackofficeDbContext", tags: new[] { "ready", "db" });

        return services;
    }

    public static IApplicationBuilder UseTenantLogContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantLogContextMiddleware>();
    }

    public static IEndpointRouteBuilder MapPersonaScriptHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Liveness probe: apenas verifica se o processo web está respondendo (não valida DBs)
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (context, report) =>
            {
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync("{\"status\":\"Healthy\"}");
            }
        }).AllowAnonymous();

        // Readiness probe: verifica conectividade dos bancos de dados
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        }).AllowAnonymous();

        // Full diagnostic health endpoint
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        }).AllowAnonymous();

        return endpoints;
    }
}
