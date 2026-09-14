using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Commands.CreateCheckoutSession;
using PersonaScript.Modules.Billing.Application.Commands.CreateCustomerPortalSession;
using PersonaScript.Modules.Billing.Application.Commands.InitializeTenantSubscription;
using PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices;
using PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails;
using PersonaScript.Modules.Billing.Application.Queries.GetTenantQuotaUsage;
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

        services.AddScoped<CreateCheckoutSessionCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>, CreateCheckoutSessionCommandHandler>();

        services.AddScoped<CreateCustomerPortalSessionCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCustomerPortalSessionCommand, CustomerPortalDto>, CreateCustomerPortalSessionCommandHandler>();

        services.AddScoped<ProcessStripeWebhookCommandHandler>();
        services.AddScoped<ICommandHandler<ProcessStripeWebhookCommand>, ProcessStripeWebhookCommandHandler>();

        services.AddScoped<InitializeTenantSubscriptionCommandHandler>();
        services.AddScoped<ICommandHandler<InitializeTenantSubscriptionCommand, Guid>, InitializeTenantSubscriptionCommandHandler>();

        services.AddScoped<PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota.ConsumeQuotaCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota.ConsumeQuotaCommand, Guid>, PersonaScript.Modules.Billing.Application.Commands.ConsumeQuota.ConsumeQuotaCommandHandler>();

        services.AddScoped<GetTenantQuotaUsageQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantQuotaUsageQuery, TenantQuotaUsageDto>, GetTenantQuotaUsageQueryHandler>();

        services.AddScoped<GetSubscriptionDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetSubscriptionDetailsQuery, SubscriptionDetailsDto>, GetSubscriptionDetailsQueryHandler>();

        services.AddScoped<GetBillingInvoicesQueryHandler>();
        services.AddScoped<IQueryHandler<GetBillingInvoicesQuery, List<InvoiceDto>>, GetBillingInvoicesQueryHandler>();

        services.AddHostedService<BackgroundServices.MonthlyQuotaResetBackgroundService>();


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
