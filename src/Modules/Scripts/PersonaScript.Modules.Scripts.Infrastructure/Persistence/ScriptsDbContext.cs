using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Infrastructure.Persistence;

public sealed class ScriptsDbContext(DbContextOptions<ScriptsDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public ITenantContext TenantContext => tenantContext;
    public DbSet<VideoScript> VideoScripts => Set<VideoScript>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("scripts");

        modelBuilder.Entity<VideoScript>(entity =>
        {
            entity.ToTable("VideoScripts");
            entity.HasKey(s => s.Id);

            entity.Property(s => s.TenantId).IsRequired();
            entity.HasIndex(s => s.TenantId);

            entity.Property(s => s.AnamneseId).IsRequired();
            entity.Property(s => s.PersonaDiagnosisId);

            entity.Property(s => s.Tema).IsRequired().HasMaxLength(500);
            entity.Property(s => s.PilarConteudo).HasMaxLength(200);
            entity.Property(s => s.Objetivo).HasMaxLength(300);

            entity.Property(s => s.Gancho).IsRequired();
            entity.Property(s => s.Retencao).IsRequired();
            entity.Property(s => s.ChamadaParaAcao).IsRequired();

            entity.Property(s => s.LegendaSugerida);
            entity.Property(s => s.DicasGravacao);
            entity.Property(s => s.TomVozAplicado).HasMaxLength(200);

            entity.Property(s => s.Status).IsRequired().HasConversion<int>();
            entity.Property(s => s.GeradoEm).IsRequired();
            entity.Property(s => s.AtualizadoEm);
        });

        modelBuilder.ApplyTenantQueryFilters(this);
    }
}
