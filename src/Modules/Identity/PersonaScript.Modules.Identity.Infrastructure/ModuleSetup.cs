using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Modules.Identity.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        return services;
    }
}
