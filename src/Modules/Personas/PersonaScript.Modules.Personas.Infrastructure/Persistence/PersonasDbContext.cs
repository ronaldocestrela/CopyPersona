using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Personas.Infrastructure.Persistence;

public sealed class PersonasDbContext(DbContextOptions<PersonasDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public ITenantContext TenantContext => tenantContext;
    public DbSet<PersonaDiagnosis> PersonaDiagnoses => Set<PersonaDiagnosis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("personas");

        modelBuilder.Entity<PersonaDiagnosis>(entity =>
        {
            entity.ToTable("PersonaDiagnoses");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.TenantId).IsRequired();
            entity.HasIndex(p => p.TenantId);

            entity.Property(p => p.AnamneseId).IsRequired();
            entity.Property(p => p.FrasePosicionamento).IsRequired();
            entity.Property(p => p.SintesePerfil).IsRequired();
            entity.Property(p => p.GeradoEm).IsRequired();
            entity.Property(p => p.AtualizadoEm);

            entity.OwnsOne(p => p.IdentidadeMarca, b => b.ToJson());
            entity.OwnsMany(p => p.PilaresConteudo, b => b.ToJson());
            entity.OwnsOne(p => p.MatrizRestricoes, b => b.ToJson());
        });

        modelBuilder.ApplyTenantQueryFilters(this);
    }
}
