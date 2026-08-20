using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;

namespace PersonaScript.Modules.Billing.Infrastructure.Repositories;

public sealed class PlanRepository(BillingDbContext dbContext) : IPlanRepository
{
    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Plan?> GetByTypeAsync(PlanType planType, CancellationToken cancellationToken = default)
    {
        return await dbContext.Plans.FirstOrDefaultAsync(p => p.PlanType == planType && p.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Plans.Where(p => p.IsActive).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Plans.ToListAsync(cancellationToken);
    }


    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        await dbContext.Plans.AddAsync(plan, cancellationToken);
    }

    public void Update(Plan plan)
    {
        dbContext.Plans.Update(plan);
    }
}
