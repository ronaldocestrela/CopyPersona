using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Queries.GetTenantQuotaUsage;

public record GetTenantQuotaUsageQuery : IQuery<TenantQuotaUsageDto>;

public sealed class GetTenantQuotaUsageQueryHandler(
    ITenantContext tenantContext,
    IUsageQuotaRepository quotaRepository)
    : IQueryHandler<GetTenantQuotaUsageQuery, TenantQuotaUsageDto>
{
    public async Task<Result<TenantQuotaUsageDto>> Handle(GetTenantQuotaUsageQuery query, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TenantQuotaUsageDto>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var quota = await quotaRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (quota == null)
        {
            return Result.Failure<TenantQuotaUsageDto>(DomainErrors.UsageQuota.NotFound);
        }

        var dto = new TenantQuotaUsageDto(
            quota.Id,
            quota.TenantId,
            quota.SubscriptionId,
            quota.PeriodStart,
            quota.PeriodEnd,
            quota.ScriptsGeneratedCount,
            quota.ScriptsLimit,
            quota.ActivePersonasCount,
            quota.ActivePersonasLimit,
            quota.AiAnalysesCount,
            quota.AiAnalysesLimit,
            quota.LastResetAt);

        return Result.Success(dto);
    }
}
