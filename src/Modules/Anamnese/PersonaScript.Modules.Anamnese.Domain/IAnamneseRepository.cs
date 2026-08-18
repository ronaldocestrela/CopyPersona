using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Anamnese.Domain;

public interface IAnamneseRepository
{
    Task<Anamnese?> GetByTenantIdAsync(CancellationToken cancellationToken = default);
    Task<Anamnese?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Anamnese anamnese, CancellationToken cancellationToken = default);
    void Update(Anamnese anamnese);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
