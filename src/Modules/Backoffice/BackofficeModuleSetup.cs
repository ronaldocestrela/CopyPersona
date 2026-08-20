using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Backoffice.Application.Commands.FreezeAccount;
using PersonaScript.Modules.Backoffice.Application.Commands.GrantExtraCredits;
using PersonaScript.Modules.Backoffice.Application.Commands.Impersonation;
using PersonaScript.Modules.Backoffice.Application.Commands.ResetPassword;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Queries.GetAuditLogs;
using PersonaScript.Modules.Backoffice.Application.Queries.GetTenantDetails;
using PersonaScript.Modules.Backoffice.Application.Queries.GetTenants;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

namespace PersonaScript.Modules.Backoffice;

public static class BackofficeModuleSetup
{
    public static IServiceCollection AddBackofficeModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<BackofficeDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
        else
        {
            services.AddDbContext<BackofficeDbContext>(options =>
                options.UseInMemoryDatabase("PersonaScript_Backoffice_InMemory"));
        }

        services.AddScoped<IAdminImpersonationLogRepository, AdminImpersonationLogRepository>();
        services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();

        // Handlers CQRS
        services.AddScoped<IQueryHandler<GetTenantsQuery, GetTenantsResult>, GetTenantsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantDetailsQuery, TenantDetailsDto>, GetTenantDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>, GetAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics.GetFinancialMetricsQuery, FinancialMetricsDto>, PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics.GetFinancialMetricsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans.GetAllPlansQuery, IReadOnlyList<PlanDto>>, PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans.GetAllPlansQueryHandler>();

        services.AddScoped<ICommandHandler<StartImpersonationCommand, Guid>, StartImpersonationCommandHandler>();
        services.AddScoped<ICommandHandler<StopImpersonationCommand>, StopImpersonationCommandHandler>();
        services.AddScoped<ICommandHandler<FreezeTenantAccountCommand>, FreezeTenantAccountCommandHandler>();
        services.AddScoped<ICommandHandler<UnfreezeTenantAccountCommand>, UnfreezeTenantAccountCommandHandler>();
        services.AddScoped<ICommandHandler<AdminResetUserPasswordCommand>, AdminResetUserPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<GrantTenantExtraCreditsCommand>, GrantTenantExtraCreditsCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits.UpdatePlanLimitsCommand>, PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits.UpdatePlanLimitsCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota.OverrideTenantQuotaCommand>, PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota.OverrideTenantQuotaCommandHandler>();

        return services;
    }
}

