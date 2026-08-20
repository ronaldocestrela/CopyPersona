using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Queries.GetTenantDetails;

public record GetTenantDetailsQuery(Guid TenantId) : IQuery<TenantDetailsDto>;

public sealed class GetTenantDetailsQueryHandler(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IUsageQuotaRepository usageQuotaRepository,
    IPlanRepository planRepository,
    IAdminAuditLogRepository auditLogRepository) : IQueryHandler<GetTenantDetailsQuery, TenantDetailsDto>
{
    public async Task<Result<TenantDetailsDto>> Handle(GetTenantDetailsQuery query, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.TenantId == query.TenantId);

        if (user is null)
        {
            return Result.Failure<TenantDetailsDto>(Error.NotFound("GetTenantDetails.UserNotFound", "Tenant/Usuário não encontrado."));
        }

        var subscription = await subscriptionRepository.GetByTenantIdAsync(user.TenantId, cancellationToken);
        var quota = await usageQuotaRepository.GetByTenantIdAsync(user.TenantId, cancellationToken);
        var plans = await planRepository.GetAllActiveAsync(cancellationToken);

        var planName = "Free";
        var subStatus = subscription?.Status.ToString() ?? "Trial";

        if (subscription != null)
        {
            var plan = plans.FirstOrDefault(p => p.Id == subscription.PlanId);
            if (plan != null)
            {
                planName = plan.Name;
            }
        }

        var summary = new TenantSummaryDto(
            TenantId: user.TenantId,
            FullName: user.FullName,
            Email: user.Email,
            Role: user.Role.ToString(),
            PlanName: planName,
            SubscriptionStatus: subStatus,
            CreatedAt: user.CreatedAt,
            IsFrozen: user.IsFrozen,
            FreezeReason: user.FreezeReason,
            ScriptsGeneratedCount: quota?.ScriptsGeneratedCount ?? 0,
            ScriptsLimit: quota?.ScriptsLimit ?? 10,
            AiAnalysesCount: quota?.AiAnalysesCount ?? 0,
            AiAnalysesLimit: quota?.AiAnalysesLimit ?? 5);

        var rawAuditLogs = await auditLogRepository.GetLogsByTargetTenantIdAsync(query.TenantId, cancellationToken);
        var auditDtos = rawAuditLogs.Select(a => new AuditLogDto(
            Id: a.Id,
            ActionType: a.ActionType,
            AdminUserId: a.AdminUserId,
            AdminEmail: a.AdminEmail,
            TargetTenantId: a.TargetTenantId,
            TargetUserEmail: a.TargetUserEmail,
            DetailsJson: a.DetailsJson,
            Timestamp: a.Timestamp)).ToList();

        // Note: Anamnese, diagnoses and scripts counts can be hydrated or zero-defaulted gracefully
        var details = new TenantDetailsDto(
            Summary: summary,
            Anamnese: null,
            DiagnosesCount: 0,
            ScriptsCount: quota?.ScriptsGeneratedCount ?? 0,
            AuditHistory: auditDtos);

        return Result.Success(details);
    }
}
