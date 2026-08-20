using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Infrastructure.Repositories;

namespace PersonaScript.Modules.Billing.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddBillingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<BillingDbContext>((sp, options) =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", "billing"));
            }
            else
            {
                options.UseInMemoryDatabase("PersonaScriptBillingDb");
            }

            var interceptor = sp.GetRequiredService<TenantDbContextInterceptor>();
            options.AddInterceptors(interceptor);
        });

        services.Configure<Application.Options.StripeOptions>(configuration.GetSection(Application.Options.StripeOptions.SectionName));

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUsageQuotaRepository, UsageQuotaRepository>();
        services.AddScoped<IQuotaTransactionRepository, QuotaTransactionRepository>();
        services.AddScoped<IProcessedStripeEventRepository, ProcessedStripeEventRepository>();
        services.AddScoped<Application.Abstractions.IStripePaymentService, Services.StripePaymentService>();
        services.AddScoped<Application.Commands.CreateCheckoutSession.CreateCheckoutSessionCommandHandler>();
        services.AddScoped<Application.Commands.CreateCustomerPortalSession.CreateCustomerPortalSessionCommandHandler>();
        services.AddScoped<Application.Commands.ProcessStripeWebhook.ProcessStripeWebhookCommandHandler>();

        return services;
    }

    public static async Task ApplyBillingMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
    }
}
