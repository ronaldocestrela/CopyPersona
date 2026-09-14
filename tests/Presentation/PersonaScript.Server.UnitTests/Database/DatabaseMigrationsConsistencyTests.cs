using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Infrastructure.Migrations;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Migrations;
using PersonaScript.Modules.Billing.Infrastructure.Migrations;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Infrastructure.Persistence.Migrations;
using PersonaScript.Modules.Personas.Infrastructure.Migrations;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Infrastructure.Migrations;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;

namespace PersonaScript.Server.UnitTests.Database;

public class DatabaseMigrationsConsistencyTests
{
    private sealed class DummyTenantContext : ITenantContext
    {
        public TenantId TenantId => TenantId.New();
    }

    [Fact]
    public void IdentityDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new IdentityDbContext(options, new DummyTenantContext());
        var userEntity = db.Model.FindEntityType(typeof(PersonaScript.Modules.Identity.Domain.User))!;
        userEntity.Should().NotBeNull();

        var snapshot = new IdentityDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();
        var snapshotEntity = snapshot.Model.FindEntityType(typeof(PersonaScript.Modules.Identity.Domain.User))!;
        snapshotEntity.Should().NotBeNull();

        var dbIndexes = userEntity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
        var snapshotIndexes = snapshotEntity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

        snapshotIndexes.Should().Contain(dbIndexes);
    }

    [Fact]
    public void AnamneseDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AnamneseDbContext(options, new DummyTenantContext());
        var anamneseEntity = db.Model.FindEntityType(typeof(PersonaScript.Modules.Anamnese.Domain.Anamnese))!;
        anamneseEntity.Should().NotBeNull();

        var snapshot = new AnamneseDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();
        var snapshotEntity = snapshot.Model.FindEntityType(typeof(PersonaScript.Modules.Anamnese.Domain.Anamnese))!;
        snapshotEntity.Should().NotBeNull();

        var dbIndexes = anamneseEntity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
        var snapshotIndexes = snapshotEntity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

        snapshotIndexes.Should().Contain(dbIndexes);
    }

    [Fact]
    public void BillingDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new BillingDbContext(options, new DummyTenantContext());
        var snapshot = new BillingDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();

        foreach (var entity in db.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            var snapshotEntity = snapshot.Model.FindEntityType(entity.ClrType);
            snapshotEntity.Should().NotBeNull($"Entity {entity.ClrType.Name} should exist in snapshot");

            var dbIndexes = entity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
            var snapshotIndexes = snapshotEntity!.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

            if (dbIndexes.Count > 0)
            {
                snapshotIndexes.Should().Contain(dbIndexes, $"Entity {entity.ClrType.Name} indexes should match");
            }
        }
    }

    [Fact]
    public void PersonasDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<PersonasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new PersonasDbContext(options, new DummyTenantContext());
        var snapshot = new PersonasDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();

        foreach (var entity in db.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            var snapshotEntity = snapshot.Model.FindEntityType(entity.ClrType);
            snapshotEntity.Should().NotBeNull($"Entity {entity.ClrType.Name} should exist in snapshot");

            var dbIndexes = entity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
            var snapshotIndexes = snapshotEntity!.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

            if (dbIndexes.Count > 0)
            {
                snapshotIndexes.Should().Contain(dbIndexes, $"Entity {entity.ClrType.Name} indexes should match");
            }
        }
    }

    [Fact]
    public void ScriptsDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<ScriptsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new ScriptsDbContext(options, new DummyTenantContext());
        var snapshot = new ScriptsDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();

        foreach (var entity in db.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            var snapshotEntity = snapshot.Model.FindEntityType(entity.ClrType);
            snapshotEntity.Should().NotBeNull($"Entity {entity.ClrType.Name} should exist in snapshot");

            var dbIndexes = entity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
            var snapshotIndexes = snapshotEntity!.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

            if (dbIndexes.Count > 0)
            {
                snapshotIndexes.Should().Contain(dbIndexes, $"Entity {entity.ClrType.Name} indexes should match");
            }
        }
    }

    [Fact]
    public void BackofficeDbContext_ModelSnapshot_ShouldBeConsistentWithDbContext()
    {
        var options = new DbContextOptionsBuilder<BackofficeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new BackofficeDbContext(options);
        var snapshot = new BackofficeDbContextModelSnapshot();
        snapshot.Model.Should().NotBeNull();

        foreach (var entity in db.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            var snapshotEntity = snapshot.Model.FindEntityType(entity.ClrType);
            snapshotEntity.Should().NotBeNull($"Entity {entity.ClrType.Name} should exist in snapshot");

            var dbIndexes = entity.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();
            var snapshotIndexes = snapshotEntity!.GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name))).ToList();

            if (dbIndexes.Count > 0)
            {
                snapshotIndexes.Should().Contain(dbIndexes, $"Entity {entity.ClrType.Name} indexes should match");
            }
        }
    }
}
