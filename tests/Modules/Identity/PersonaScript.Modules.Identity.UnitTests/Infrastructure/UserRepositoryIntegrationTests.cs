using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Repositories;

namespace PersonaScript.Modules.Identity.UnitTests.Infrastructure;

public class UserRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistUserWithTenantIdEqualToId()
    {
        var tenantContext = new FixedTenantContext(TenantId.From(Guid.Empty));
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new IdentityDbContext(options, tenantContext);
        var repository = new UserRepository(dbContext);

        var userResult = User.Register("Maria Silva", "maria@example.com", "hash");
        userResult.IsSuccess.Should().BeTrue();

        await repository.AddAsync(userResult.Value, CancellationToken.None);

        var stored = await dbContext.Users.IgnoreQueryFilters().SingleAsync();
        stored.TenantId.Should().Be(stored.Id);
        stored.Email.Should().Be("maria@example.com");
    }
}
