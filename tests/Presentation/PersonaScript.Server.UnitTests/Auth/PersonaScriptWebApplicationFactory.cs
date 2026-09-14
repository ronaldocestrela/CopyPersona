using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;

namespace PersonaScript.Server.UnitTests.Auth;

public sealed class PersonaScriptWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("APPLY_MIGRATIONS", "false");
        builder.UseSetting("ApplyMigrationsOnStartup", "false");
        builder.UseSetting("ConnectionStrings:DefaultConnection", string.Empty);
        builder.UseSetting("LLM:PrimaryProvider", "Mock");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<PersonaScript.Modules.Anamnese.Infrastructure.Persistence.AnamneseDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<PersonaScript.Modules.Personas.Infrastructure.Persistence.PersonasDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<PersonaScript.Modules.Scripts.Infrastructure.Persistence.ScriptsDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<PersonaScript.Modules.Billing.Infrastructure.Persistence.BillingDbContext>().Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<PersonaScript.Modules.Backoffice.Infrastructure.Persistence.BackofficeDbContext>().Database.EnsureCreated();

        return host;
    }
}
