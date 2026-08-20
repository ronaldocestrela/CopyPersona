using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Commands.CreateCustomerPortalSession;

public record CreateCustomerPortalSessionCommand(
    string? ReturnUrl = null) : ICommand<CustomerPortalDto>;

public class CreateCustomerPortalSessionCommandHandler(
    ISubscriptionRepository subscriptionRepository,
    IStripePaymentService stripePaymentService,
    ITenantContext tenantContext) : ICommandHandler<CreateCustomerPortalSessionCommand, CustomerPortalDto>
{
    public async Task<Result<CustomerPortalDto>> Handle(
        CreateCustomerPortalSessionCommand command,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<CustomerPortalDto>(Error.Unauthorized("CreateCustomerPortalSession.Unauthorized", "Usuário/Tenant não autenticado."));
        }

        var subscription = await subscriptionRepository.GetByTenantIdAsync(tenantContext.TenantId.Value, cancellationToken);
        if (subscription is null)
        {
            return Result.Failure<CustomerPortalDto>(DomainErrors.Subscription.NotFound);
        }

        if (string.IsNullOrWhiteSpace(subscription.StripeCustomerId))
        {
            return Result.Failure<CustomerPortalDto>(DomainErrors.Stripe.NoCustomer);
        }

        return await stripePaymentService.CreateCustomerPortalSessionAsync(
            subscription.StripeCustomerId,
            command.ReturnUrl,
            cancellationToken);
    }
}
