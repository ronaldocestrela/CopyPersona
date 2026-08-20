using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

public sealed class BackofficeDbContext : DbContext
{
    public BackofficeDbContext(DbContextOptions<BackofficeDbContext> options) : base(options)
    {
    }

    public DbSet<AdminImpersonationLog> ImpersonationLogs => Set<AdminImpersonationLog>();
    public DbSet<AdminAuditLog> AuditLogs => Set<AdminAuditLog>();
    public DbSet<PromptTemplate> PromptTemplates => Set<PromptTemplate>();
    public DbSet<AgentExecutionLog> AgentExecutionLogs => Set<AgentExecutionLog>();
    public DbSet<CouncilRule> CouncilRules => Set<CouncilRule>();
    public DbSet<ForbiddenTerm> ForbiddenTerms => Set<ForbiddenTerm>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminImpersonationLog>(builder =>
        {
            builder.ToTable("AdminImpersonationLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AdminEmail).HasMaxLength(256).IsRequired();
            builder.Property(x => x.TargetUserEmail).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.IpAddress).HasMaxLength(45);
        });

        modelBuilder.Entity<AdminAuditLog>(builder =>
        {
            builder.ToTable("AdminAuditLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ActionType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AdminEmail).HasMaxLength(256).IsRequired();
            builder.Property(x => x.TargetUserEmail).HasMaxLength(256).IsRequired();
            builder.Property(x => x.DetailsJson).IsRequired();
        });

        modelBuilder.Entity<PromptTemplate>(builder =>
        {
            builder.ToTable("PromptTemplates");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AgentName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SystemPrompt).IsRequired();
            builder.Property(x => x.UserPromptTemplate).IsRequired();
            builder.Property(x => x.ParametersJson).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
            builder.Property(x => x.CreatedByAdminEmail).HasMaxLength(256).IsRequired();
            builder.HasIndex(x => new { x.AgentName, x.Version }).IsUnique();
            builder.HasIndex(x => new { x.AgentName, x.IsActive });
        });

        modelBuilder.Entity<AgentExecutionLog>(builder =>
        {
            builder.ToTable("AgentExecutionLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.AgentName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ModelUsed).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ProviderType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EstimatedCostUSD).HasColumnType("decimal(18,6)");
            builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
            builder.HasIndex(x => x.ExecutedAtUtc);
            builder.HasIndex(x => x.TenantId);
            builder.HasIndex(x => new { x.AgentName, x.Status });
        });

        modelBuilder.Entity<CouncilRule>(builder =>
        {
            builder.ToTable("CouncilRules");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CouncilAcronym).HasMaxLength(20).IsRequired();
            builder.Property(x => x.CouncilName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.ResolutionNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.GuidelinesText).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
            builder.HasIndex(x => x.CouncilAcronym);
            builder.HasIndex(x => new { x.CouncilAcronym, x.IsActive });
        });

        modelBuilder.Entity<ForbiddenTerm>(builder =>
        {
            builder.ToTable("ForbiddenTerms");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Term).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ReplacementSuggestion).HasMaxLength(200);
            builder.Property(x => x.Reasoning).HasMaxLength(1000);
            builder.HasIndex(x => x.Term);
            builder.HasIndex(x => x.IsActive);
        });

        base.OnModelCreating(modelBuilder);
    }
}
