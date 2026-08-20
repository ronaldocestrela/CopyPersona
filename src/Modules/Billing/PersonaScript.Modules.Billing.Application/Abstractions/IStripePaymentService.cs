using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.Abstractions;

public interface IStripePaymentService
{
    Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(
        Guid tenantId,
        string customerEmail,
        Plan plan,
        string? customSuccessUrl = null,
        string? customCancelUrl = null,
        CancellationToken cancellationToken = default);

    Task<Result<CustomerPortalDto>> CreateCustomerPortalSessionAsync(
        string stripeCustomerId,
        string? customReturnUrl = null,
        CancellationToken cancellationToken = default);

    Task<Result<List<InvoiceDto>>> GetCustomerInvoicesAsync(
        string stripeCustomerId,
        CancellationToken cancellationToken = default);

    Result<ProcessStripeWebhookCommand> ParseWebhookEvent(string jsonPayload, string signatureHeader);
}

