using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateContentPlan;
using PersonaScript.Modules.Scripts.Application.Commands.GenerateVideoScript;
using PersonaScript.Modules.Scripts.Application.Commands.UpdateVideoScriptStatus;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Queries.GetNinetyDayCalendar;
using PersonaScript.Modules.Scripts.Application.Queries.GetStoryPlan;
using PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;
using PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;

namespace PersonaScript.Modules.Scripts.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddScriptsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ScriptsDbContext>((serviceProvider, options) =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("PersonaScriptScriptsDb");
            }

            var interceptor = serviceProvider.GetRequiredService<TenantDbContextInterceptor>();
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IVideoScriptRepository, VideoScriptRepository>();
        services.AddScoped<IStoryPlanRepository, StoryPlanRepository>();
        services.AddScoped<INinetyDayCalendarRepository, NinetyDayCalendarRepository>();

        // Register Services & Prompt Builder
        services.AddSingleton<IVideoScriptPromptBuilder, VideoScriptPromptBuilder>();
        services.AddScoped<IVideoScriptGenerator, VideoScriptGenerator>();
        services.AddSingleton<IContentPlanPromptBuilder, ContentPlanPromptBuilder>();
        services.AddScoped<IContentPlanGenerator, ContentPlanGenerator>();

        // Register CQRS Handlers
        services.AddScoped<ICommandHandler<GenerateVideoScriptCommand, Guid>, GenerateVideoScriptCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateVideoScriptStatusCommand>, UpdateVideoScriptStatusCommandHandler>();
        services.AddScoped<ICommandHandler<GenerateContentPlanCommand, ContentPlanResultDto>, GenerateContentPlanCommandHandler>();
        services.AddScoped<IQueryHandler<GetVideoScriptByIdQuery, VideoScriptDto>, GetVideoScriptByIdQueryHandler>();
        services.AddScoped<IQueryHandler<ListVideoScriptsQuery, IReadOnlyList<VideoScriptDto>>, ListVideoScriptsQueryHandler>();
        services.AddScoped<IQueryHandler<GetStoryPlanQuery, StoryPlanDto>, GetStoryPlanQueryHandler>();
        services.AddScoped<IQueryHandler<GetNinetyDayCalendarQuery, NinetyDayCalendarDto>, GetNinetyDayCalendarQueryHandler>();

        return services;
    }

    public static async Task ApplyScriptsMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ScriptsDbContext>();
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
