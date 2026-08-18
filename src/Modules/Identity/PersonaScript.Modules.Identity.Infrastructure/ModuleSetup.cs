using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Application.Commands.LoginUser;
using PersonaScript.Modules.Identity.Application.Commands.RegisterUser;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Repositories;
using PersonaScript.Modules.Identity.Infrastructure.Security;

namespace PersonaScript.Modules.Identity.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            if (environment.IsEnvironment("Testing"))
            {
                options.UseInMemoryDatabase("PersonaScriptAuthTests");
                return;
            }

            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();
        services.AddScoped<IAuthSession, CookieAuthSession>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, LoginResult>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<LoginUserCommand, LoginResult>, LoginUserCommandHandler>();

        return services;
    }

    public static async Task ApplyIdentityMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
