using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;

public sealed class StoryPlanRepository : IStoryPlanRepository
{
    private readonly ScriptsDbContext _context;

    public StoryPlanRepository(ScriptsDbContext context)
    {
        _context = context;
    }

    public async Task<StoryPlan?> GetByTenantIdAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _context.TenantContext.TenantId.Value;
        return await _context.StoryPlans
            .FirstOrDefaultAsync(sp => sp.TenantId == tenantId, cancellationToken);
    }

    public async Task<StoryPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StoryPlans
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task AddAsync(StoryPlan plan, CancellationToken cancellationToken = default)
    {
        await _context.StoryPlans.AddAsync(plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(StoryPlan plan, CancellationToken cancellationToken = default)
    {
        _context.StoryPlans.Update(plan);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
