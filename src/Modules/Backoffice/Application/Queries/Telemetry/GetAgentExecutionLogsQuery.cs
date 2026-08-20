using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Telemetry;

public sealed record GetAgentExecutionLogsQuery(
    int Page = 1,
    int PageSize = 20,
    string? AgentFilter = null,
    string? ModelFilter = null,
    AgentExecutionStatus? StatusFilter = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IQuery<GetAgentExecutionLogsResult>;

public sealed record GetAgentExecutionLogsResult(
    IReadOnlyList<AgentExecutionLogDto> Logs,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class GetAgentExecutionLogsQueryHandler : IQueryHandler<GetAgentExecutionLogsQuery, GetAgentExecutionLogsResult>
{
    private readonly IAgentExecutionLogRepository _repository;

    public GetAgentExecutionLogsQueryHandler(IAgentExecutionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetAgentExecutionLogsResult>> Handle(GetAgentExecutionLogsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPagedLogsAsync(
            query.Page,
            query.PageSize,
            query.AgentFilter,
            query.ModelFilter,
            query.StatusFilter,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var dtos = items.Select(x => new AgentExecutionLogDto
        {
            Id = x.Id,
            TenantId = x.TenantId,
            AgentName = x.AgentName,
            ModelUsed = x.ModelUsed,
            ProviderType = x.ProviderType,
            PromptTokens = x.PromptTokens,
            CompletionTokens = x.CompletionTokens,
            TotalTokens = x.TotalTokens,
            EstimatedCostUSD = x.EstimatedCostUSD,
            LatencyMs = x.LatencyMs,
            Status = x.Status.ToString(),
            ErrorMessage = x.ErrorMessage,
            ExecutedAtUtc = x.ExecutedAtUtc
        }).ToList();

        var result = new GetAgentExecutionLogsResult(dtos, totalCount, query.Page, query.PageSize);
        return Result.Success(result);
    }
}
