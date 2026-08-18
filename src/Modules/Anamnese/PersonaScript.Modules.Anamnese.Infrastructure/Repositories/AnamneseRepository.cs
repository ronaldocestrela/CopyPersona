using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Infrastructure.Persistence;

namespace PersonaScript.Modules.Anamnese.Infrastructure.Repositories;

public sealed class AnamneseRepository(AnamneseDbContext dbContext) : IAnamneseRepository
{
    public async Task<Domain.Anamnese?> GetByTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Anamneses
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Domain.Anamnese?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Anamneses
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Domain.Anamnese anamnese, CancellationToken cancellationToken = default)
    {
        await dbContext.Anamneses.AddAsync(anamnese, cancellationToken);
    }

    public void Update(Domain.Anamnese anamnese)
    {
        dbContext.Anamneses.Update(anamnese);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
