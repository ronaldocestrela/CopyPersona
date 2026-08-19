namespace PersonaScript.Modules.Scripts.Domain;

public interface INinetyDayCalendarRepository
{
    Task<NinetyDayCalendar?> GetByTenantIdAsync(CancellationToken cancellationToken = default);
    Task<NinetyDayCalendar?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(NinetyDayCalendar calendar, CancellationToken cancellationToken = default);
    Task UpdateAsync(NinetyDayCalendar calendar, CancellationToken cancellationToken = default);
}
