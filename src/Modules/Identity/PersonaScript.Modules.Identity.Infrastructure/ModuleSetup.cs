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

using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Identity.Application.Commands.RequestPasswordReset;
using PersonaScript.Modules.Identity.Application.Commands.ResetPassword;
using PersonaScript.Modules.Identity.Infrastructure.Emails;

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

        services.AddHttpClient<ResendEmailSender>();

        if (environment.IsEnvironment("Testing") || string.IsNullOrWhiteSpace(configuration["Resend:ApiKey"]))
        {
            services.AddSingleton<IEmailSender, FakeEmailSender>();
        }
        else
        {
            services.AddScoped<IEmailSender, ResendEmailSender>();
        }

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, AspNetPasswordHasher>();
        services.AddScoped<IAuthSession, CookieAuthSession>();
        services.AddScoped<ICommandHandler<RegisterUserCommand, LoginResult>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<LoginUserCommand, LoginResult>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<RequestPasswordResetCommand>, RequestPasswordResetCommandHandler>();
        services.AddScoped<ICommandHandler<ResetPasswordCommand>, ResetPasswordCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Identity.Application.Commands.ExternalLogin.ExternalLoginCommand, LoginResult>, PersonaScript.Modules.Identity.Application.Commands.ExternalLogin.ExternalLoginCommandHandler>();
        services.AddScoped<ICommandHandler<PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken.GenerateJwtTokenCommand, JwtTokenResult>, PersonaScript.Modules.Identity.Application.Commands.GenerateJwtToken.GenerateJwtTokenCommandHandler>();

        return services;
    }

    public static async Task ApplyIdentityMigrationsAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
