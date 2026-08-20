namespace PersonaScript.Modules.Billing.Domain;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Plan?> GetByTypeAsync(PlanType planType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
    void Update(Plan plan);
}
