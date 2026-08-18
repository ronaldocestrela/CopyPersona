using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Anamnese.Infrastructure.Repositories;

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

        return services;
    }
}
