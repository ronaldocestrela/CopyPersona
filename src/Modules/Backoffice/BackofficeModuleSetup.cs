using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Modules.Backoffice;

public static class BackofficeModuleSetup
{
    public static IServiceCollection AddBackofficeModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Registro de serviços do módulo de Backoffice para as subfases da Fase 6
        return services;
    }
}
