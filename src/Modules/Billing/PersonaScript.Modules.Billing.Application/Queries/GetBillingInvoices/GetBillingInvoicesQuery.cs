using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices;

public record GetBillingInvoicesQuery : IQuery<List<InvoiceDto>>;

public class GetBillingInvoicesQueryHandler(
    ITenantContext tenantContext,
    ISubscriptionRepository subscriptionRepository,
    IStripePaymentService stripePaymentService)
    : IQueryHandler<GetBillingInvoicesQuery, List<InvoiceDto>>
{
    public virtual async Task<Result<List<InvoiceDto>>> Handle(GetBillingInvoicesQuery query, CancellationToken cancellationToken)

    {
        var tenantId = tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<List<InvoiceDto>>(Error.Unauthorized("Billing.TenantIdInvalid", "Tenant não autenticado."));
        }

        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        if (subscription == null || string.IsNullOrWhiteSpace(subscription.StripeCustomerId))
        {
            return Result.Success(new List<InvoiceDto>());
        }

        return await stripePaymentService.GetCustomerInvoicesAsync(subscription.StripeCustomerId, cancellationToken);
    }
}
