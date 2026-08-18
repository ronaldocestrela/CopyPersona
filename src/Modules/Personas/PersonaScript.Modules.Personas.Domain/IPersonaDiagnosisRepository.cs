namespace PersonaScript.Modules.Personas.Domain;

public interface IPersonaDiagnosisRepository
{
    Task<PersonaDiagnosis?> GetByTenantIdAsync(CancellationToken cancellationToken = default);
    Task<PersonaDiagnosis?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PersonaDiagnosis diagnosis, CancellationToken cancellationToken = default);
    void Update(PersonaDiagnosis diagnosis);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
