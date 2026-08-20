using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Repositories;

public sealed class AgentExecutionLogRepository : IAgentExecutionLogRepository
{
    private readonly BackofficeDbContext _dbContext;

    public AgentExecutionLogRepository(BackofficeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(AgentExecutionLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.AgentExecutionLogs.AddAsync(log, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<AgentExecutionLog> Items, int TotalCount)> GetPagedLogsAsync(
        int page,
        int pageSize,
        string? agentFilter = null,
        string? modelFilter = null,
        AgentExecutionStatus? statusFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AgentExecutionLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(agentFilter))
        {
            query = query.Where(x => x.AgentName.Contains(agentFilter));
        }

        if (!string.IsNullOrWhiteSpace(modelFilter))
        {
            query = query.Where(x => x.ModelUsed.Contains(modelFilter));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(x => x.Status == statusFilter.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.ExecutedAtUtc >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.ExecutedAtUtc <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.ExecutedAtUtc)
            .Skip((Math.Max(1, page) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<AgentExecutionLog>> GetLogsInPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AgentExecutionLogs
            .AsNoTracking()
            .Where(x => x.ExecutedAtUtc >= startDate && x.ExecutedAtUtc <= endDate)
            .OrderByDescending(x => x.ExecutedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
