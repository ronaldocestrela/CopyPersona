using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Billing.Infrastructure.Persistence;

namespace PersonaScript.Modules.Billing.Infrastructure.Repositories;

public class ProcessedStripeEventRepository(BillingDbContext dbContext) : IProcessedStripeEventRepository
{
    public async Task<ProcessedStripeEvent?> GetByIdAsync(string eventId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ProcessedStripeEvent>()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
    }

    public async Task AddAsync(ProcessedStripeEvent processedEvent, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<ProcessedStripeEvent>().AddAsync(processedEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
