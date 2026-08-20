using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Domain.Repositories;

public interface IAgentExecutionLogRepository
{
    Task AddAsync(AgentExecutionLog log, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AgentExecutionLog> Items, int TotalCount)> GetPagedLogsAsync(
        int page,
        int pageSize,
        string? agentFilter = null,
        string? modelFilter = null,
        AgentExecutionStatus? statusFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentExecutionLog>> GetLogsInPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
