using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Tenancy;

namespace PersonaScript.BuildingBlocks.UnitTests.Tenancy;

public class TenancyIntegrationTests
{
    private class DocumentEntity : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; private set; }
        public string Title { get; set; } = string.Empty;

        public void SetTenantId(Guid tenantId) => TenantId = tenantId;
    }

    private class MutableTenantContext : ITenantContext
    {
        public TenantId TenantId { get; set; } = TenantId.From(Guid.Empty);
    }

    private class ApplicationDbContext : DbContext
    {
        private readonly ITenantContext _tenantContext;
        private readonly TenantDbContextInterceptor _interceptor;

        public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext)
            : base(options)
        {
            _tenantContext = tenantContext;
            _interceptor = new TenantDbContextInterceptor(tenantContext);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(_interceptor);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyTenantQueryFilters(this);
        }
    }

    [Fact]
    public async Task MultiTenancy_ShouldIsolateDataAndAutoAssignTenantId()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var tenantA = TenantId.New();
        var tenantB = TenantId.New();

        var tenantContext = new MutableTenantContext();

        // Tenant A inserts data
        tenantContext.TenantId = tenantA;
        using (var dbA = new ApplicationDbContext(options, tenantContext))
        {
            dbA.Documents.Add(new DocumentEntity { Title = "Doc Tenant A - 1" });
            dbA.Documents.Add(new DocumentEntity { Title = "Doc Tenant A - 2" });
            await dbA.SaveChangesAsync();
        }

        // Tenant B inserts data
        tenantContext.TenantId = tenantB;
        using (var dbB = new ApplicationDbContext(options, tenantContext))
        {
            dbB.Documents.Add(new DocumentEntity { Title = "Doc Tenant B - 1" });
            await dbB.SaveChangesAsync();
        }

        // Query as Tenant A
        tenantContext.TenantId = tenantA;
        using (var dbAQuery = new ApplicationDbContext(options, tenantContext))
        {
            var docsA = await dbAQuery.Documents.ToListAsync();
            docsA.Should().HaveCount(2);
            docsA.Should().AllSatisfy(d => d.TenantId.Should().Be(tenantA.Value));
            docsA.Select(d => d.Title).Should().Contain(["Doc Tenant A - 1", "Doc Tenant A - 2"]);
        }

        // Query as Tenant B
        tenantContext.TenantId = tenantB;
        using (var dbBQuery = new ApplicationDbContext(options, tenantContext))
        {
            var docsB = await dbBQuery.Documents.ToListAsync();
            docsB.Should().HaveCount(1);
            docsB.Should().AllSatisfy(d => d.TenantId.Should().Be(tenantB.Value));
            docsB.Single().Title.Should().Be("Doc Tenant B - 1");
        }
    }

    [Fact]
    public async Task IgnoreQueryFilters_ShouldReturnAllTenantsData()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var tenantA = TenantId.New();
        var tenantB = TenantId.New();

        var tenantContext = new MutableTenantContext();

        tenantContext.TenantId = tenantA;
        using (var dbA = new ApplicationDbContext(options, tenantContext))
        {
            dbA.Documents.Add(new DocumentEntity { Title = "Doc A" });
            await dbA.SaveChangesAsync();
        }

        tenantContext.TenantId = tenantB;
        using (var dbB = new ApplicationDbContext(options, tenantContext))
        {
            dbB.Documents.Add(new DocumentEntity { Title = "Doc B" });
            await dbB.SaveChangesAsync();
        }

        tenantContext.TenantId = tenantA;
        using (var dbQuery = new ApplicationDbContext(options, tenantContext))
        {
            var allDocs = await dbQuery.Documents.IgnoreQueryFilters().ToListAsync();
            allDocs.Should().HaveCount(2);
        }
    }
}
