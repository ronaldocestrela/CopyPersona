using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class TenantIndexingModelTests
{
    private class TestItem : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public string Name { get; set; } = string.Empty;
        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }

    private class NonTenantItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestItem> TestItems => Set<TestItem>();
        public DbSet<NonTenantItem> NonTenantItems => Set<NonTenantItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.EnsureTenantIndexes();
        }
    }

    [Fact]
    public void EnsureTenantIndexes_ShouldCreateAnIndexOnTenantId_ForEntitiesImplementingIMustHaveTenant()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new TestDbContext(options);
        var entityType = db.Model.FindEntityType(typeof(TestItem));
        entityType.Should().NotBeNull();

        var indexes = entityType!.GetIndexes().ToList();
        var hasTenantIndex = indexes.Any(idx => idx.Properties.Any(p => p.Name == nameof(IMustHaveTenant.TenantId)));

        hasTenantIndex.Should().BeTrue("All entities implementing IMustHaveTenant must have an index on TenantId");
    }

    [Fact]
    public void EnsureTenantIndexes_ShouldNotCreateIndex_ForEntitiesNotImplementingIMustHaveTenant()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new TestDbContext(options);
        var entityType = db.Model.FindEntityType(typeof(NonTenantItem));
        entityType.Should().NotBeNull();

        var indexes = entityType!.GetIndexes().ToList();
        var hasTenantIndex = indexes.Any(idx => idx.Properties.Any(p => p.Name == nameof(IMustHaveTenant.TenantId)));

        hasTenantIndex.Should().BeFalse();
    }
}
