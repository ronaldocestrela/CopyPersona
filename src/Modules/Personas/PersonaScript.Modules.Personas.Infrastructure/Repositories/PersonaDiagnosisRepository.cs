using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Infrastructure.Persistence;

namespace PersonaScript.Modules.Personas.Infrastructure.Repositories;

public sealed class PersonaDiagnosisRepository(PersonasDbContext dbContext) : IPersonaDiagnosisRepository
{
    public async Task<PersonaDiagnosis?> GetByTenantIdAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PersonaDiagnoses
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PersonaDiagnosis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.PersonaDiagnoses
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(PersonaDiagnosis diagnosis, CancellationToken cancellationToken = default)
    {
        await dbContext.PersonaDiagnoses.AddAsync(diagnosis, cancellationToken);
    }

    public void Update(PersonaDiagnosis diagnosis)
    {
        dbContext.PersonaDiagnoses.Update(diagnosis);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
