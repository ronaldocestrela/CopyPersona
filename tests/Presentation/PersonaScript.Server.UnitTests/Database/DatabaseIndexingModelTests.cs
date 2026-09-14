using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;
using PersonaScript.Modules.Identity.Domain;
using PersonaScript.Modules.Identity.Infrastructure.Persistence;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Infrastructure.Persistence;

namespace PersonaScript.Server.UnitTests.Database;

public class DatabaseIndexingModelTests
{
    private class DummyTenantContext : ITenantContext
    {
        public TenantId TenantId => TenantId.New();
    }

    [Fact]
    public void IdentityDbContext_ShouldHaveTenantAndCompositeIndexes()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new IdentityDbContext(options, new DummyTenantContext());
        var entityType = db.Model.FindEntityType(typeof(User))!;
        entityType.Should().NotBeNull();

        var indexes = entityType.GetIndexes().ToList();
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "Role" }));
    }

    [Fact]
    public void AnamneseDbContext_ShouldHaveTenantAndStatusIndexes()
    {
        var options = new DbContextOptionsBuilder<AnamneseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AnamneseDbContext(options, new DummyTenantContext());
        var entityType = db.Model.FindEntityType(typeof(PersonaScript.Modules.Anamnese.Domain.Anamnese))!;
        entityType.Should().NotBeNull();

        var indexes = entityType.GetIndexes().ToList();
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "Status" }));
    }

    [Fact]
    public void PersonasDbContext_ShouldHaveTenantAndCompositeIndexes()
    {
        var options = new DbContextOptionsBuilder<PersonasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new PersonasDbContext(options, new DummyTenantContext());
        var entityType = db.Model.FindEntityType(typeof(PersonaDiagnosis))!;
        entityType.Should().NotBeNull();

        var indexes = entityType.GetIndexes().ToList();
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "GeradoEm" }));
        indexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "AnamneseId" }));
    }

    [Fact]
    public void ScriptsDbContext_ShouldHaveTenantAndCompositeIndexes()
    {
        var options = new DbContextOptionsBuilder<ScriptsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new ScriptsDbContext(options, new DummyTenantContext());

        var scriptType = db.Model.FindEntityType(typeof(VideoScript))!;
        var scriptIndexes = scriptType.GetIndexes().ToList();
        scriptIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        scriptIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "GeradoEm" }));
        scriptIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "Status", "GeradoEm" }));
        scriptIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "AnamneseId" }));

        var storyType = db.Model.FindEntityType(typeof(StoryPlan))!;
        var storyIndexes = storyType.GetIndexes().ToList();
        storyIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        storyIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "GeradoEm" }));
        storyIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "AnamneseId" }));

        var calendarType = db.Model.FindEntityType(typeof(NinetyDayCalendar))!;
        var calendarIndexes = calendarType.GetIndexes().ToList();
        calendarIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        calendarIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "GeradoEm" }));
        calendarIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "AnamneseId" }));
    }

    [Fact]
    public void BillingDbContext_ShouldHaveTenantAndCompositeIndexes()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new BillingDbContext(options, new DummyTenantContext());

        var subType = db.Model.FindEntityType(typeof(Subscription))!;
        var subIndexes = subType.GetIndexes().ToList();
        subIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        subIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "Status" }));

        var quotaType = db.Model.FindEntityType(typeof(UsageQuota))!;
        var quotaIndexes = quotaType.GetIndexes().ToList();
        quotaIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        quotaIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "PeriodEnd" }));

        var transType = db.Model.FindEntityType(typeof(QuotaTransaction))!;
        var transIndexes = transType.GetIndexes().ToList();
        transIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId" }));
        transIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TenantId", "TransactionDate" }));
    }

    [Fact]
    public void BackofficeDbContext_ShouldHaveAuditAndImpersonationIndexes()
    {
        var options = new DbContextOptionsBuilder<BackofficeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new BackofficeDbContext(options);

        var auditType = db.Model.FindEntityType(typeof(AdminAuditLog))!;
        var auditIndexes = auditType.GetIndexes().ToList();
        auditIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TargetTenantId" }));
        auditIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "ActionType", "Timestamp" }));

        var impType = db.Model.FindEntityType(typeof(AdminImpersonationLog))!;
        var impIndexes = impType.GetIndexes().ToList();
        impIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TargetTenantId" }));
        impIndexes.Should().Contain(idx => idx.Properties.Select(p => p.Name).SequenceEqual(new[] { "TargetUserEmail", "StartedAt" }));
    }
}
