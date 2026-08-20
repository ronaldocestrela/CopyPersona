namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface IForbiddenTermRepository
{
    Task<IReadOnlyList<ForbiddenTerm>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ForbiddenTerm>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ForbiddenTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ForbiddenTerm term, CancellationToken cancellationToken = default);
    Task UpdateAsync(ForbiddenTerm term, CancellationToken cancellationToken = default);
    Task DeleteAsync(ForbiddenTerm term, CancellationToken cancellationToken = default);
}
