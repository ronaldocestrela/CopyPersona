using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Modules.Scripts.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddScriptsModule(this IServiceCollection services)
    {
        return services;
    }
}
