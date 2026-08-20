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

        base.OnModelCreating(modelBuilder);
    }
}
