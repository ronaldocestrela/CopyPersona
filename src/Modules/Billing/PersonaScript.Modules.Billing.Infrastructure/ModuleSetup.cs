using Microsoft.Extensions.DependencyInjection;

namespace PersonaScript.Modules.Billing.Infrastructure;

public static class ModuleSetup
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services)
    {
        return services;
    }
}
