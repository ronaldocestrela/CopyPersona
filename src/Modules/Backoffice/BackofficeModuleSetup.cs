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
        services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
        services.AddScoped<IAgentExecutionLogRepository, AgentExecutionLogRepository>();
        services.AddScoped<ICouncilRuleRepository, CouncilRuleRepository>();
        services.AddScoped<IForbiddenTermRepository, ForbiddenTermRepository>();
        services.AddSingleton<PersonaScript.Modules.Backoffice.Application.Services.ILLMCostCalculator, PersonaScript.Modules.Backoffice.Application.Services.LLMCostCalculator>();
        services.AddSingleton<PersonaScript.Modules.Backoffice.Application.Abstractions.ILLMTelemetryService, PersonaScript.Modules.Backoffice.Application.Services.LLMTelemetryService>();
        services.AddScoped<PersonaScript.Modules.Backoffice.Application.Services.IDynamicPromptEngine, PersonaScript.Modules.Backoffice.Application.Services.DynamicPromptEngine>();
        services.AddScoped<PersonaScript.Modules.Backoffice.Application.Services.IQualityModeratorService, PersonaScript.Modules.Backoffice.Application.Services.QualityModeratorService>();

        // Handlers CQRS
        services.AddScoped<IQueryHandler<GetTenantsQuery, GetTenantsResult>, GetTenantsQueryHandler>();
        services.AddScoped<IQueryHandler<GetTenantDetailsQuery, TenantDetailsDto>, GetTenantDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>, GetAuditLogsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics.GetFinancialMetricsQuery, FinancialMetricsDto>, PersonaScript.Modules.Backoffice.Application.Queries.GetFinancialMetrics.GetFinancialMetricsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans.GetAllPlansQuery, IReadOnlyList<PlanDto>>, PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans.GetAllPlansQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Prompts.GetPromptTemplatesQuery, IReadOnlyList<PromptTemplateDto>>, PersonaScript.Modules.Backoffice.Application.Queries.Prompts.GetPromptTemplatesQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Prompts.GetPromptHistoryQuery, IReadOnlyList<PromptTemplateDto>>, PersonaScript.Modules.Backoffice.Application.Queries.Prompts.GetPromptHistoryQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetTelemetrySummaryQuery, TelemetrySummaryDto>, PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetTelemetrySummaryQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetAgentExecutionLogsQuery, PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetAgentExecutionLogsResult>, PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetAgentExecutionLogsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetAnomalyAlertsQuery, IReadOnlyList<AnomalyAlertDto>>, PersonaScript.Modules.Backoffice.Application.Queries.Telemetry.GetAnomalyAlertsQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Compliance.GetCouncilRulesQuery, IReadOnlyList<CouncilRuleDto>>, PersonaScript.Modules.Backoffice.Application.Queries.Compliance.GetCouncilRulesQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Backoffice.Application.Queries.Compliance.GetForbiddenTermsQuery, IReadOnlyList<ForbiddenTermDto>>, PersonaScript.Modules.Backoffice.Application.Queries.Compliance.GetForbiddenTermsQueryHandler>();

        services.AddScoped<ICommandHandler<StartImpersonationCommand, Guid>, StartImpersonationCommandHandler>();
        services.AddScoped<ICommandHandler<StopImpersonationCommand>, StopImpersonationCommandHandler>();
        services.AddScoped<ICommandHandler<FreezeTenantAccountCommand>, FreezeTenantAccountCommandHandler>();
        services.AddScoped<ICommandHandler<UnfreezeTenantAccountCommand>, UnfreezeTenantAccountCommandHandler>();
        services.AddScoped<ICommandHandler<AdminResetUserPasswordCommand>, AdminResetUserPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<GrantTenantExtraCreditsCommand>, GrantTenantExtraCreditsCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits.UpdatePlanLimitsCommand>, PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits.UpdatePlanLimitsCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota.OverrideTenantQuotaCommand>, PersonaScript.Modules.Backoffice.Application.Commands.OverrideTenantQuota.OverrideTenantQuotaCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Prompts.CreatePromptVersionCommand, Guid>, PersonaScript.Modules.Backoffice.Application.Commands.Prompts.CreatePromptVersionCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Prompts.RollbackPromptVersionCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Prompts.RollbackPromptVersionCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Prompts.TestPromptPlaygroundCommand, TestPromptResultDto>, PersonaScript.Modules.Backoffice.Application.Commands.Prompts.TestPromptPlaygroundCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.CreateCouncilRuleCommand, Guid>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.CreateCouncilRuleCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.UpdateCouncilRuleCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.UpdateCouncilRuleCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ToggleCouncilRuleStatusCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ToggleCouncilRuleStatusCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.CreateForbiddenTermCommand, Guid>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.CreateForbiddenTermCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.UpdateForbiddenTermCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.UpdateForbiddenTermCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ToggleForbiddenTermStatusCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ToggleForbiddenTermStatusCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.DeleteForbiddenTermCommand>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.DeleteForbiddenTermCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ModerateContentCommand, QualityModerationResultDto>, PersonaScript.Modules.Backoffice.Application.Commands.Compliance.ModerateContentCommandHandler>();

        return services;
    }

    public static async Task ApplyBackofficeMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BackofficeDbContext>();

        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await PromptTemplateSeeder.SeedDefaultPromptsAsync(dbContext, cancellationToken);
        await EthicalGovernanceSeeder.SeedAsync(dbContext, cancellationToken);
    }
}

