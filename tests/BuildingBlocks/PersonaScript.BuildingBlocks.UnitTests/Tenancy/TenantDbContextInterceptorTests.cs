using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class TenantDbContextInterceptorTests
{
    private class TestTenantEntity : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public string Name { get; set; } = string.Empty;

        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }

    private class TestDbContext(DbContextOptions<TestDbContext> options, TenantDbContextInterceptor interceptor) : DbContext(options)
    {
        public DbSet<TestTenantEntity> TenantEntities => Set<TestTenantEntity>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(interceptor);
        }
    }

    private static TestDbContext CreateDbContext(ITenantContext tenantContext)
    {
        var interceptor = new TenantDbContextInterceptor(tenantContext);
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options, interceptor);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldAutoAssignTenantId_WhenEntityHasEmptyTenantId()
    {
        var tenantId = TenantId.New();
        var context = CreateDbContext(new FixedTenantContext(tenantId));

        var entity = new TestTenantEntity { Name = "Test" };
        context.TenantEntities.Add(entity);
        await context.SaveChangesAsync();

        entity.TenantId.Should().Be(tenantId.Value);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException_WhenAddingEntityWithoutActiveTenantContext()
    {
        var context = CreateDbContext(new NullTenantContext());

        var entity = new TestTenantEntity { Name = "Test" };
        context.TenantEntities.Add(entity);

        var act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*without an active authenticated TenantContext*");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException_WhenEntityTenantIdMismatchActiveContext()
    {
        var activeTenantId = TenantId.New();
        var foreignTenantId = TenantId.New();
        var context = CreateDbContext(new FixedTenantContext(activeTenantId));

        var entity = new TestTenantEntity { Name = "Test" };
        entity.SetTenantId(foreignTenantId.Value);
        context.TenantEntities.Add(entity);

        var act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*under active TenantContext*");
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException_WhenTenantIdIsModified()
    {
        var tenantId = TenantId.New();
        var context = CreateDbContext(new FixedTenantContext(tenantId));

        var entity = new TestTenantEntity { Name = "Initial" };
        context.TenantEntities.Add(entity);
        await context.SaveChangesAsync();

        // Attempt to modify TenantId
        entity.SetTenantId(Guid.NewGuid());
        context.Entry(entity).Property(e => e.TenantId).IsModified = true;

        var act = async () => await context.SaveChangesAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Cannot modify TenantId*");
    }
}
