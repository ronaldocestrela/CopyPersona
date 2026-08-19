using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Infrastructure.Persistence.Repositories;

public sealed class NinetyDayCalendarRepository : INinetyDayCalendarRepository
{
    private readonly ScriptsDbContext _context;

    public NinetyDayCalendarRepository(ScriptsDbContext context)
    {
        _context = context;
    }

    public async Task<NinetyDayCalendar?> GetByTenantIdAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _context.TenantContext.TenantId.Value;
        return await _context.NinetyDayCalendars
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, cancellationToken);
    }

    public async Task<NinetyDayCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NinetyDayCalendars
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(NinetyDayCalendar calendar, CancellationToken cancellationToken = default)
    {
        await _context.NinetyDayCalendars.AddAsync(calendar, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(NinetyDayCalendar calendar, CancellationToken cancellationToken = default)
    {
        _context.NinetyDayCalendars.Update(calendar);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
