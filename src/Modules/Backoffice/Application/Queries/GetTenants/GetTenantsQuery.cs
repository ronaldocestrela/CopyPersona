using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;
using PersonaScript.Modules.Identity.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Queries.GetTenants;

public record GetTenantsQuery(
    string? SearchTerm = null,
    string? StatusFilter = null,
    string? PlanFilter = null,
    int Page = 1,
    int PageSize = 20) : IQuery<GetTenantsResult>;

public record GetTenantsResult(
    IReadOnlyList<TenantSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed class GetTenantsQueryHandler(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    IUsageQuotaRepository usageQuotaRepository,
    IPlanRepository planRepository) : IQueryHandler<GetTenantsQuery, GetTenantsResult>
{
    public async Task<Result<GetTenantsResult>> Handle(GetTenantsQuery query, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        var plans = await planRepository.GetAllActiveAsync(cancellationToken);

        var queryableUsers = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLowerInvariant();
            queryableUsers = queryableUsers.Where(u =>
                u.Email.ToLowerInvariant().Contains(term) ||
                u.FullName.ToLowerInvariant().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.StatusFilter))
        {
            if (query.StatusFilter.Equals("Frozen", StringComparison.OrdinalIgnoreCase))
            {
                queryableUsers = queryableUsers.Where(u => u.IsFrozen);
            }
            else if (query.StatusFilter.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                queryableUsers = queryableUsers.Where(u => !u.IsFrozen);
            }
        }

        var filteredUsers = queryableUsers.ToList();
        var totalCount = filteredUsers.Count;
        var pagedUsers = filteredUsers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var summaries = new List<TenantSummaryDto>();

        foreach (var user in pagedUsers)
        {
            var subscription = await subscriptionRepository.GetByTenantIdAsync(user.TenantId, cancellationToken);
            var quota = await usageQuotaRepository.GetByTenantIdAsync(user.TenantId, cancellationToken);

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

            summaries.Add(new TenantSummaryDto(
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
                AiAnalysesLimit: quota?.AiAnalysesLimit ?? 5));
        }

        return Result.Success(new GetTenantsResult(summaries, totalCount, query.Page, query.PageSize));
    }
}
