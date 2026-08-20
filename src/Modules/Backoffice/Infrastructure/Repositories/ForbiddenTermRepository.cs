using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class ForbiddenTermRepository : IForbiddenTermRepository
{
    private readonly BackofficeDbContext _dbContext;

    public ForbiddenTermRepository(BackofficeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ForbiddenTerm>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ForbiddenTerms
            .Where(t => t.IsActive)
            .OrderBy(t => t.Term)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ForbiddenTerm>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ForbiddenTerms
            .OrderBy(t => t.Term)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ForbiddenTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ForbiddenTerms.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        await _dbContext.ForbiddenTerms.AddAsync(term, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        _dbContext.ForbiddenTerms.Update(term);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ForbiddenTerm term, CancellationToken cancellationToken = default)
    {
        _dbContext.ForbiddenTerms.Remove(term);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
