using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Modules.Personas.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddPersonasModule(this IServiceCollection services)
    {
        return services;
    }
}
