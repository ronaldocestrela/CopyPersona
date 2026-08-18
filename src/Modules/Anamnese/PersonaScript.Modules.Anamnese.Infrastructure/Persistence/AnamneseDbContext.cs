using Microsoft.EntityFrameworkCore;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Infrastructure.Persistence;

public sealed class AnamneseDbContext(DbContextOptions<AnamneseDbContext> options, ITenantContext tenantContext)
    : DbContext(options)
{
    public ITenantContext TenantContext => tenantContext;
    public DbSet<Domain.Anamnese> Anamneses => Set<Domain.Anamnese>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("anamnese");

        modelBuilder.Entity<Domain.Anamnese>(entity =>
        {
            entity.ToTable("Anamneses");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.TenantId).IsRequired();
            entity.HasIndex(a => a.TenantId);

            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(a => a.EtapaAtual).IsRequired();
            entity.Property(a => a.PercentualConclusao).IsRequired();
            entity.Property(a => a.CriadoEm).IsRequired();
            entity.Property(a => a.AtualizadoEm);
            entity.Property(a => a.ConcluidoEm);

            entity.OwnsOne(a => a.Etapa1, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa2, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa3, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa4, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa5, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa6, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa7, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa8, b =>
            {
                b.ToJson();
                b.Property(e => e.ArquetiposComunicacao)
                    .HasConversion(
                        v => string.Join(',', v.Select(a => a.ToString())),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(Enum.Parse<ArquetipoComunicacaoEnum>)
                              .ToList()
                    );
            });
            entity.OwnsOne(a => a.Etapa9, b => b.ToJson());
            entity.OwnsOne(a => a.Etapa10, b => b.ToJson());
        });

        modelBuilder.ApplyTenantQueryFilters(this);
    }
}
