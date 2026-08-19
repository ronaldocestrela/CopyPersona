using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Infrastructure.Persistence;

public sealed class ScriptsDbContext(DbContextOptions<ScriptsDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public ITenantContext TenantContext => tenantContext;
    public DbSet<VideoScript> VideoScripts => Set<VideoScript>();
    public DbSet<StoryPlan> StoryPlans => Set<StoryPlan>();
    public DbSet<NinetyDayCalendar> NinetyDayCalendars => Set<NinetyDayCalendar>();

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

        modelBuilder.Entity<StoryPlan>(entity =>
        {
            entity.ToTable("StoryPlans");
            entity.HasKey(sp => sp.Id);

            entity.Property(sp => sp.TenantId).IsRequired();
            entity.HasIndex(sp => sp.TenantId);

            entity.Property(sp => sp.AnamneseId).IsRequired();
            entity.Property(sp => sp.PersonaDiagnosisId);

            entity.Property(sp => sp.FrequenciaDiariaRecomendada).IsRequired().HasMaxLength(200);
            entity.Property(sp => sp.DiretrizesHumanizacao);
            entity.Property(sp => sp.GeradoEm).IsRequired();
            entity.Property(sp => sp.AtualizadoEm);

            entity.OwnsMany(sp => sp.BlocosHorarios, b => b.ToJson());
        });

        modelBuilder.Entity<NinetyDayCalendar>(entity =>
        {
            entity.ToTable("NinetyDayCalendars");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.TenantId).IsRequired();
            entity.HasIndex(c => c.TenantId);

            entity.Property(c => c.AnamneseId).IsRequired();
            entity.Property(c => c.PersonaDiagnosisId);

            entity.Property(c => c.ObjetivoTrimestral).IsRequired().HasMaxLength(300);
            entity.Property(c => c.GeradoEm).IsRequired();
            entity.Property(c => c.AtualizadoEm);

            entity.OwnsMany(c => c.Semanas, s => s.ToJson());
        });

        modelBuilder.ApplyTenantQueryFilters(this);
    }
}
