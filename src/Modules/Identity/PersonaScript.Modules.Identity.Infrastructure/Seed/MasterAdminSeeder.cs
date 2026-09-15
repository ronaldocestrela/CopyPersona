using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;

namespace PersonaScript.Modules.Identity.Infrastructure.Seed;

public sealed class MasterAdminSeeder(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    ILogger<MasterAdminSeeder> logger)
{
    private const int MinimumPasswordLength = 8;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var adminEmail = configuration["MASTER_ADMIN_EMAIL"] ?? configuration["MasterAdmin:Email"];
        var adminPassword = configuration["MASTER_ADMIN_PASSWORD"] ?? configuration["MasterAdmin:Password"];
        var adminName = configuration["MASTER_ADMIN_NAME"] ?? configuration["MasterAdmin:FullName"] ?? "Master Administrator";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogInformation("MasterAdminSeeder: Nenhuma credencial de administrador master configurada em MASTER_ADMIN_EMAIL / MASTER_ADMIN_PASSWORD. Seed ignorado.");
            return;
        }

        if (adminPassword.Length < MinimumPasswordLength)
        {
            logger.LogWarning("MasterAdminSeeder: A senha configurada para o usuário master possui menos de {MinLength} caracteres. Seed abortado por segurança.", MinimumPasswordLength);
            return;
        }

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();

        var existingUser = await dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.Role != UserRole.SystemAdmin)
            {
                existingUser.AssignRole(UserRole.SystemAdmin);
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogInformation("MasterAdminSeeder: Usuário existente {Email} promovido para o papel SystemAdmin.", normalizedEmail);
            }
            else
            {
                logger.LogInformation("MasterAdminSeeder: Usuário master {Email} já existe e possui o papel SystemAdmin.", normalizedEmail);
            }
            return;
        }

        var passwordHash = passwordHasher.HashPassword(adminPassword);
        var registerResult = User.Register(adminName, normalizedEmail, passwordHash);

        if (registerResult.IsFailure)
        {
            logger.LogWarning("MasterAdminSeeder: Falha ao validar dados para o usuário master: {Error}", registerResult.Error.Message);
            return;
        }

        var user = registerResult.Value;
        user.AssignRole(UserRole.SystemAdmin);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("MasterAdminSeeder: Usuário master {Email} criado com sucesso com papel SystemAdmin e TenantId {TenantId}.", normalizedEmail, user.TenantId);
    }
}
