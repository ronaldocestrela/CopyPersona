namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface ICouncilRuleRepository
{
    Task<CouncilRule?> GetByAcronymAsync(string councilAcronym, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CouncilRule>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CouncilRule>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CouncilRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(CouncilRule rule, CancellationToken cancellationToken = default);
    Task UpdateAsync(CouncilRule rule, CancellationToken cancellationToken = default);
}
