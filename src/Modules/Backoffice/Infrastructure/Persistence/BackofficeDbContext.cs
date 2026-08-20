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

        base.OnModelCreating(modelBuilder);
    }
}
