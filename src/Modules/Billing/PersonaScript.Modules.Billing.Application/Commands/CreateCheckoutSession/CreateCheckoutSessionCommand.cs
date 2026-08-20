using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Commands.CreateCheckoutSession;

public record CreateCheckoutSessionCommand(
    PlanType PlanType,
    string CustomerEmail,
    string? SuccessUrl = null,
    string? CancelUrl = null) : ICommand<CheckoutSessionDto>;

public class CreateCheckoutSessionCommandHandler(
    IPlanRepository planRepository,
    IStripePaymentService stripePaymentService,
    ITenantContext tenantContext) : ICommandHandler<CreateCheckoutSessionCommand, CheckoutSessionDto>
{
    public virtual async Task<Result<CheckoutSessionDto>> Handle(
        CreateCheckoutSessionCommand command,
        CancellationToken cancellationToken)

    {
        if (tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<CheckoutSessionDto>(Error.Unauthorized("CreateCheckoutSession.Unauthorized", "Usuário/Tenant não autenticado."));
        }

        if (string.IsNullOrWhiteSpace(command.CustomerEmail))
        {
            return Result.Failure<CheckoutSessionDto>(Error.Validation("CreateCheckoutSession.EmailRequired", "O email do cliente é obrigatório."));
        }

        var plan = await planRepository.GetByTypeAsync(command.PlanType, cancellationToken);
        if (plan is null || !plan.IsActive)
        {
            return Result.Failure<CheckoutSessionDto>(DomainErrors.Plan.NotFound);
        }

        return await stripePaymentService.CreateCheckoutSessionAsync(
            tenantContext.TenantId.Value,
            command.CustomerEmail,
            plan,
            command.SuccessUrl,
            command.CancelUrl,
            cancellationToken);
    }
}
