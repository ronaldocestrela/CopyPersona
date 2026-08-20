using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class CouncilRuleRepository : ICouncilRuleRepository
{
    private readonly BackofficeDbContext _dbContext;

    public CouncilRuleRepository(BackofficeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CouncilRule?> GetByAcronymAsync(string councilAcronym, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(councilAcronym))
            return null;

        var normalized = councilAcronym.Trim().ToUpperInvariant();
        return await _dbContext.CouncilRules
            .FirstOrDefaultAsync(r => r.IsActive && r.CouncilAcronym == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<CouncilRule>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CouncilRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.CouncilAcronym)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CouncilRule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.CouncilRules
            .OrderBy(r => r.CouncilAcronym)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<CouncilRule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CouncilRules.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(CouncilRule rule, CancellationToken cancellationToken = default)
    {
        await _dbContext.CouncilRules.AddAsync(rule, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(CouncilRule rule, CancellationToken cancellationToken = default)
    {
        _dbContext.CouncilRules.Update(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
