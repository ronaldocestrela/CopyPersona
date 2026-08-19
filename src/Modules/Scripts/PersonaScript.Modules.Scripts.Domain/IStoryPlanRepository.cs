namespace PersonaScript.Modules.Scripts.Domain;

public interface IStoryPlanRepository
{
    Task<StoryPlan?> GetByTenantIdAsync(CancellationToken cancellationToken = default);
    Task<StoryPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(StoryPlan plan, CancellationToken cancellationToken = default);
    Task UpdateAsync(StoryPlan plan, CancellationToken cancellationToken = default);
}
