using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.GetAuditLogs;

public record GetAuditLogsQuery(int Take = 100) : IQuery<IReadOnlyList<AuditLogDto>>;

public sealed class GetAuditLogsQueryHandler(
    IAdminAuditLogRepository auditLogRepository) : IQueryHandler<GetAuditLogsQuery, IReadOnlyList<AuditLogDto>>
{
    public async Task<Result<IReadOnlyList<AuditLogDto>>> Handle(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var logs = await auditLogRepository.GetAllAsync(query.Take, cancellationToken);
        var dtos = logs.Select(a => new AuditLogDto(
            Id: a.Id,
            ActionType: a.ActionType,
            AdminUserId: a.AdminUserId,
            AdminEmail: a.AdminEmail,
            TargetTenantId: a.TargetTenantId,
            TargetUserEmail: a.TargetUserEmail,
            DetailsJson: a.DetailsJson,
            Timestamp: a.Timestamp)).ToList();

        return Result.Success<IReadOnlyList<AuditLogDto>>(dtos);
    }
}
