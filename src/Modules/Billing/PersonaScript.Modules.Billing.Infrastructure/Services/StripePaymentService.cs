using System.Text.Json;
using Microsoft.Extensions.Options;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;
using PersonaScript.Modules.Billing.Application.DTOs;
using PersonaScript.Modules.Billing.Application.Options;
using PersonaScript.Modules.Billing.Domain;
using Stripe;
using Stripe.Checkout;
using DomainPlan = PersonaScript.Modules.Billing.Domain.Plan;

namespace PersonaScript.Modules.Billing.Infrastructure.Services;

public class StripePaymentService : IStripePaymentService
{
    private readonly StripeOptions _options;

    public StripePaymentService(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            StripeConfiguration.ApiKey = _options.ApiKey;
        }
    }

    public async Task<Result<CheckoutSessionDto>> CreateCheckoutSessionAsync(
        Guid tenantId,
        string customerEmail,
        DomainPlan plan,
        string? customSuccessUrl = null,
        string? customCancelUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var priceId = plan.StripePriceId;
            if (string.IsNullOrWhiteSpace(priceId))
            {
                priceId = plan.PlanType switch
                {
                    PersonaScript.Modules.Billing.Domain.PlanType.Basic => _options.BasicPriceId,
                    PersonaScript.Modules.Billing.Domain.PlanType.Pro => _options.ProPriceId,
                    PersonaScript.Modules.Billing.Domain.PlanType.Reference => _options.ReferencePriceId,
                    _ => string.Empty
                };
            }

            if (string.IsNullOrWhiteSpace(priceId))
            {
                return Result.Failure<CheckoutSessionDto>(DomainErrors.Stripe.MissingPriceId);
            }

            var successUrl = string.IsNullOrWhiteSpace(customSuccessUrl) ? _options.SuccessUrl : customSuccessUrl;
            var cancelUrl = string.IsNullOrWhiteSpace(customCancelUrl) ? _options.CancelUrl : customCancelUrl;

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = ["card"],
                Mode = "subscription",
                CustomerEmail = customerEmail,
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                ],
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "tenant_id", tenantId.ToString() },
                    { "plan_id", plan.Id.ToString() }
                },
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "tenant_id", tenantId.ToString() },
                        { "plan_id", plan.Id.ToString() }
                    }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return Result.Success(new CheckoutSessionDto(session.Id, session.Url));
        }
        catch (StripeException ex)
        {
            return Result.Failure<CheckoutSessionDto>(Error.Failure("Stripe.CheckoutError", ex.Message));
        }
    }

    public async Task<Result<CustomerPortalDto>> CreateCustomerPortalSessionAsync(
        string stripeCustomerId,
        string? customReturnUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(stripeCustomerId))
            {
                return Result.Failure<CustomerPortalDto>(DomainErrors.Stripe.NoCustomer);
            }

            var returnUrl = string.IsNullOrWhiteSpace(customReturnUrl) ? _options.ReturnUrl : customReturnUrl;

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = stripeCustomerId,
                ReturnUrl = returnUrl
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return Result.Success(new CustomerPortalDto(session.Url));
        }
        catch (StripeException ex)
        {
            return Result.Failure<CustomerPortalDto>(Error.Failure("Stripe.PortalError", ex.Message));
        }
    }

    public async Task<Result<List<InvoiceDto>>> GetCustomerInvoicesAsync(
        string stripeCustomerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(stripeCustomerId))
            {
                return Result.Success(new List<InvoiceDto>());
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return Result.Success(new List<InvoiceDto>());
            }

            var options = new InvoiceListOptions
            {
                Customer = stripeCustomerId,
                Limit = 20
            };

            var service = new InvoiceService();
            StripeList<Stripe.Invoice> invoices = await service.ListAsync(options, cancellationToken: cancellationToken);

            var dtos = invoices.Data.Select(i => new InvoiceDto(
                InvoiceId: i.Id,
                AmountPaid: i.AmountPaid / 100m,
                Currency: i.Currency?.ToUpperInvariant() ?? "BRL",
                Status: i.Status,
                InvoicePdfUrl: i.InvoicePdf,
                CreatedAt: i.Created
            )).ToList();

            return Result.Success(dtos);
        }
        catch (StripeException ex)
        {
            return Result.Failure<List<InvoiceDto>>(Error.Failure("Stripe.InvoicesError", ex.Message));
        }
    }

    public Result<ProcessStripeWebhookCommand> ParseWebhookEvent(string jsonPayload, string signatureHeader)

    {
        try
        {
            if (!string.IsNullOrWhiteSpace(_options.WebhookSecret))
            {
                EventUtility.ConstructEvent(jsonPayload, signatureHeader, _options.WebhookSecret);
            }

            using var doc = JsonDocument.Parse(jsonPayload);
            var root = doc.RootElement;

            string eventId = root.GetProperty("id").GetString() ?? string.Empty;
            string eventType = root.GetProperty("type").GetString() ?? string.Empty;

            string stripeCustomerId = string.Empty;
            string? stripeSubscriptionId = null;
            string? stripePriceId = null;
            DateTime? periodStart = null;
            DateTime? periodEnd = null;
            Guid? tenantIdMetadata = null;

            if (root.TryGetProperty("data", out var dataEl) && dataEl.TryGetProperty("object", out var objEl))
            {
                if (objEl.TryGetProperty("customer", out var custEl) && custEl.ValueKind == JsonValueKind.String)
                {
                    stripeCustomerId = custEl.GetString() ?? string.Empty;
                }

                if (eventType.StartsWith("customer.subscription"))
                {
                    if (objEl.TryGetProperty("id", out var subIdEl) && subIdEl.ValueKind == JsonValueKind.String)
                    {
                        stripeSubscriptionId = subIdEl.GetString();
                    }
                }
                else if (objEl.TryGetProperty("subscription", out var subEl) && subEl.ValueKind == JsonValueKind.String)
                {
                    stripeSubscriptionId = subEl.GetString();
                }

                if (objEl.TryGetProperty("current_period_start", out var pStartEl) && pStartEl.TryGetInt64(out var pStartUnix))
                {
                    periodStart = DateTimeOffset.FromUnixTimeSeconds(pStartUnix).UtcDateTime;
                }
                else if (objEl.TryGetProperty("period_start", out var pStartEl2) && pStartEl2.TryGetInt64(out var pStartUnix2))
                {
                    periodStart = DateTimeOffset.FromUnixTimeSeconds(pStartUnix2).UtcDateTime;
                }

                if (objEl.TryGetProperty("current_period_end", out var pEndEl) && pEndEl.TryGetInt64(out var pEndUnix))
                {
                    periodEnd = DateTimeOffset.FromUnixTimeSeconds(pEndUnix).UtcDateTime;
                }
                else if (objEl.TryGetProperty("period_end", out var pEndEl2) && pEndEl2.TryGetInt64(out var pEndUnix2))
                {
                    periodEnd = DateTimeOffset.FromUnixTimeSeconds(pEndUnix2).UtcDateTime;
                }

                // Extrai StripePriceId de items ou lines
                if (objEl.TryGetProperty("items", out var itemsEl) && itemsEl.TryGetProperty("data", out var itemsDataEl) && itemsDataEl.ValueKind == JsonValueKind.Array && itemsDataEl.GetArrayLength() > 0)
                {
                    var firstItem = itemsDataEl[0];
                    if (firstItem.TryGetProperty("price", out var priceEl) && priceEl.TryGetProperty("id", out var priceIdEl))
                    {
                        stripePriceId = priceIdEl.GetString();
                    }
                }
                else if (objEl.TryGetProperty("lines", out var linesEl) && linesEl.TryGetProperty("data", out var linesDataEl) && linesDataEl.ValueKind == JsonValueKind.Array && linesDataEl.GetArrayLength() > 0)
                {
                    var firstLine = linesDataEl[0];
                    if (firstLine.TryGetProperty("price", out var priceEl) && priceEl.TryGetProperty("id", out var priceIdEl))
                    {
                        stripePriceId = priceIdEl.GetString();
                    }
                }

                // Extrai tenant_id metadata
                if (objEl.TryGetProperty("metadata", out var metaEl) && metaEl.TryGetProperty("tenant_id", out var tenantMetaEl) && tenantMetaEl.ValueKind == JsonValueKind.String)
                {
                    if (Guid.TryParse(tenantMetaEl.GetString(), out var tId))
                    {
                        tenantIdMetadata = tId;
                    }
                }
            }

            var command = new ProcessStripeWebhookCommand(
                EventId: eventId,
                EventType: eventType,
                StripeCustomerId: stripeCustomerId,
                StripeSubscriptionId: stripeSubscriptionId,
                StripePriceId: stripePriceId,
                PeriodStart: periodStart,
                PeriodEnd: periodEnd,
                TenantIdMetadata: tenantIdMetadata);

            return Result.Success(command);
        }
        catch (Exception)
        {
            return Result.Failure<ProcessStripeWebhookCommand>(PersonaScript.Modules.Billing.Domain.DomainErrors.Stripe.InvalidSignature);
        }
    }
}
