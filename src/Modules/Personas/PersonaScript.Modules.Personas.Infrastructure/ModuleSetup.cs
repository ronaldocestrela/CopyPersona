using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.Services;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Personas.Infrastructure.Repositories;

namespace PersonaScript.Modules.Personas.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddPersonasModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PersonasDbContext>((serviceProvider, options) =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("PersonaScriptPersonasDb");
            }

            var interceptor = serviceProvider.GetRequiredService<TenantDbContextInterceptor>();
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IPersonaDiagnosisRepository, PersonaDiagnosisRepository>();

        // Register Services & Prompt Builder
        services.AddSingleton<IPersonaPromptBuilder, PersonaPromptBuilder>();
        services.AddScoped<IPersonaDiagnosisGenerator, PersonaDiagnosisGenerator>();

        // Register CQRS Handlers
        services.AddScoped<ICommandHandler<GeneratePersonaDiagnosisCommand, Guid>, GeneratePersonaDiagnosisCommandHandler>();
        services.AddScoped<IQueryHandler<GetPersonaDiagnosisQuery, PersonaDiagnosisDto?>, GetPersonaDiagnosisQueryHandler>();

        return services;
    }

    public static async Task ApplyPersonasMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PersonasDbContext>();
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
