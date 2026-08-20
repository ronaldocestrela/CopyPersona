using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Infrastructure.Persistence;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public ITenantContext TenantContext => tenantContext;

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<UsageQuota> UsageQuotas => Set<UsageQuota>();
    public DbSet<QuotaTransaction> QuotaTransactions => Set<QuotaTransaction>();
    public DbSet<ProcessedStripeEvent> ProcessedStripeEvents => Set<ProcessedStripeEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.ToTable("Plans");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.PlanType).IsRequired();
            entity.Property(p => p.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(p => p.YearlyPrice).HasPrecision(18, 2);
            entity.Property(p => p.MaxActivePersonas).IsRequired();
            entity.Property(p => p.MaxScriptsPerMonth).IsRequired();
            entity.Property(p => p.MaxAiAnalysesPerMonth).IsRequired();
            entity.Property(p => p.IsActive).IsRequired();
            entity.Property(p => p.StripePriceId).HasMaxLength(100);
            entity.Property(p => p.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.TenantId).IsRequired();
            entity.HasIndex(s => s.TenantId);

            entity.Property(s => s.PlanId).IsRequired();
            entity.HasOne(s => s.Plan)
                  .WithMany()
                  .HasForeignKey(s => s.PlanId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(s => s.Status).IsRequired();
            entity.Property(s => s.CurrentPeriodStart).IsRequired();
            entity.Property(s => s.CurrentPeriodEnd).IsRequired();
            entity.Property(s => s.CancelAtPeriodEnd).IsRequired();
            entity.Property(s => s.StripeCustomerId).HasMaxLength(100);
            entity.Property(s => s.StripeSubscriptionId).HasMaxLength(100);
            entity.HasIndex(s => s.StripeSubscriptionId);
            entity.Property(s => s.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<UsageQuota>(entity =>
        {
            entity.ToTable("UsageQuotas");
            entity.HasKey(q => q.Id);

            entity.Property(q => q.TenantId).IsRequired();
            entity.HasIndex(q => q.TenantId);

            entity.Property(q => q.SubscriptionId).IsRequired();
            entity.Property(q => q.PeriodStart).IsRequired();
            entity.Property(q => q.PeriodEnd).IsRequired();
            entity.Property(q => q.ScriptsGeneratedCount).IsRequired();
            entity.Property(q => q.ActivePersonasCount).IsRequired();
            entity.Property(q => q.AiAnalysesCount).IsRequired();
            entity.Property(q => q.ScriptsLimit).IsRequired();
            entity.Property(q => q.ActivePersonasLimit).IsRequired();
            entity.Property(q => q.AiAnalysesLimit).IsRequired();
            entity.Property(q => q.LastResetAt).IsRequired();
        });

        modelBuilder.Entity<QuotaTransaction>(entity =>
        {
            entity.ToTable("QuotaTransactions");
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TenantId).IsRequired();
            entity.HasIndex(t => t.TenantId);

            entity.Property(t => t.QuotaId).IsRequired();
            entity.Property(t => t.ResourceType).IsRequired();
            entity.Property(t => t.Quantity).IsRequired();
            entity.Property(t => t.TransactionDate).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(250).IsRequired();
            entity.Property(t => t.SourceCommand).HasMaxLength(150);
        });

        modelBuilder.Entity<ProcessedStripeEvent>(entity =>
        {
            entity.ToTable("ProcessedStripeEvents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasMaxLength(150);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProcessedAt).IsRequired();
        });

        modelBuilder.ApplyTenantQueryFilters(this);
    }
}
