using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Identity.Application.Abstractions;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Seed;

namespace PersonaScript.Modules.Identity.UnitTests.Infrastructure;

public class MasterAdminSeederTests
{
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    public MasterAdminSeederTests()
    {
        _passwordHasher.HashPassword(Arg.Any<string>()).Returns(ci => $"hashed_{ci.Arg<string>()}");
    }

    private IdentityDbContext CreateDbContext()
    {
        var tenantContext = new FixedTenantContext(TenantId.From(Guid.Empty));
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new IdentityDbContext(options, tenantContext);
    }

    [Fact]
    public async Task SeedAsync_ShouldCreateMasterUser_WhenConfiguredAndUserDoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MASTER_ADMIN_EMAIL"] = "admin@personascript.ai",
                ["MASTER_ADMIN_PASSWORD"] = "AdminPass2026!Strong",
                ["MASTER_ADMIN_NAME"] = "Master Administrator"
            })
            .Build();

        var seeder = new MasterAdminSeeder(dbContext, _passwordHasher, config, NullLogger<MasterAdminSeeder>.Instance);

        await seeder.SeedAsync(CancellationToken.None);

        var user = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == "admin@personascript.ai");
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.SystemAdmin);
        user.FullName.Should().Be("Master Administrator");
        user.PasswordHash.Should().Be("hashed_AdminPass2026!Strong");
        user.TenantId.Should().Be(user.Id);
    }

    [Fact]
    public async Task SeedAsync_ShouldEnsureSystemAdminRole_WhenUserAlreadyExists()
    {
        await using var dbContext = CreateDbContext();
        var existingUser = User.Register("Existing Admin", "admin@personascript.ai", "existing_hash").Value;
        // starts as Subscriber
        existingUser.Role.Should().Be(UserRole.Subscriber);
        dbContext.Users.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MASTER_ADMIN_EMAIL"] = "admin@personascript.ai",
                ["MASTER_ADMIN_PASSWORD"] = "AdminPass2026!Strong"
            })
            .Build();

        var seeder = new MasterAdminSeeder(dbContext, _passwordHasher, config, NullLogger<MasterAdminSeeder>.Instance);

        await seeder.SeedAsync(CancellationToken.None);

        var users = await dbContext.Users.IgnoreQueryFilters().Where(u => u.Email == "admin@personascript.ai").ToListAsync();
        users.Should().HaveCount(1);
        users[0].Role.Should().Be(UserRole.SystemAdmin);
    }

    [Fact]
    public async Task SeedAsync_ShouldDoNothing_WhenEmailOrPasswordMissing()
    {
        await using var dbContext = CreateDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MASTER_ADMIN_EMAIL"] = "",
                ["MASTER_ADMIN_PASSWORD"] = ""
            })
            .Build();

        var seeder = new MasterAdminSeeder(dbContext, _passwordHasher, config, NullLogger<MasterAdminSeeder>.Instance);

        await seeder.SeedAsync(CancellationToken.None);

        var count = await dbContext.Users.IgnoreQueryFilters().CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task SeedAsync_ShouldDoNothing_WhenPasswordIsTooShort()
    {
        await using var dbContext = CreateDbContext();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MASTER_ADMIN_EMAIL"] = "admin@personascript.ai",
                ["MASTER_ADMIN_PASSWORD"] = "short"
            })
            .Build();

        var seeder = new MasterAdminSeeder(dbContext, _passwordHasher, config, NullLogger<MasterAdminSeeder>.Instance);

        await seeder.SeedAsync(CancellationToken.None);

        var count = await dbContext.Users.IgnoreQueryFilters().CountAsync();
        count.Should().Be(0);
    }
}
