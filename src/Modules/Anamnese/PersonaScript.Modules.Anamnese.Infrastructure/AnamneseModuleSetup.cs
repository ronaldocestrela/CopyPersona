using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;
using PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;
using PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStep;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;
using PersonaScript.Modules.Anamnese.Application.Services;

namespace PersonaScript.Modules.Anamnese.Infrastructure;

public static class AnamneseModuleSetup
{
    public static IServiceCollection AddAnamneseModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AnamneseDbContext>((serviceProvider, options) =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("PersonaScriptAnamneseDb");
            }

            var interceptor = serviceProvider.GetRequiredService<TenantDbContextInterceptor>();
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IAnamneseRepository, AnamneseRepository>();

        // Register Clarification AI Engine
        services.AddSingleton<HeuristicClarificationAnalyzer>();
        services.AddScoped<IAnamneseClarificationService, AnamneseClarificationService>();

        // Register CQRS Handlers
        services.AddScoped<ICommandHandler<StartAnamneseCommand, Guid>, StartAnamneseCommandHandler>();
        services.AddScoped<ICommandHandler<SaveAnamneseStepCommand>, SaveAnamneseStepCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteAnamneseCommand>, CompleteAnamneseCommandHandler>();

        services.AddScoped<IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>, GetAnamneseStatusQueryHandler>();
        services.AddScoped<IQueryHandler<GetAnamneseStepQuery, object?>, GetAnamneseStepQueryHandler>();
        services.AddScoped<IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>, GetFullAnamneseQueryHandler>();
        services.AddScoped<IQueryHandler<PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification.AnalyzeStepClarificationQuery, PersonaScript.Modules.Anamnese.Application.DTOs.ClarificationAnalysisResultDto>, PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification.AnalyzeStepClarificationQueryHandler>();

        return services;
    }

    public static async Task ApplyAnamneseMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AnamneseDbContext>();
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
