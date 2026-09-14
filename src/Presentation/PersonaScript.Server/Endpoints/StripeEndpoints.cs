using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PersonaScript.Modules.Billing.Application.Abstractions;
using PersonaScript.Modules.Billing.Application.Commands.CreateCheckoutSession;
using PersonaScript.Modules.Billing.Application.Commands.CreateCustomerPortalSession;
using PersonaScript.Modules.Billing.Application.Commands.ProcessStripeWebhook;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Server.Endpoints;

public static class StripeEndpoints
{
    public static IEndpointRouteBuilder MapStripeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/billing");

        group.MapPost("/checkout", async (
            [FromBody] CreateCheckoutRequest request,
            [FromServices] CreateCheckoutSessionCommandHandler handler,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userEmail = httpContext.User.FindFirstValue(ClaimTypes.Email) ?? "user@example.com";
            var command = new CreateCheckoutSessionCommand(
                request.PlanType,
                userEmail,
                request.SuccessUrl,
                request.CancelUrl);

            var result = await handler.Handle(command, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapPost("/portal", async (
            [FromBody] CreatePortalRequest request,
            [FromServices] CreateCustomerPortalSessionCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCustomerPortalSessionCommand(request.ReturnUrl);
            var result = await handler.Handle(command, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet("/subscription", async (
            [FromServices] PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails.GetSubscriptionDetailsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new PersonaScript.Modules.Billing.Application.Queries.GetSubscriptionDetails.GetSubscriptionDetailsQuery();
            var result = await handler.Handle(query, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();

        group.MapGet("/invoices", async (
            [FromServices] PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices.GetBillingInvoicesQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new PersonaScript.Modules.Billing.Application.Queries.GetBillingInvoices.GetBillingInvoicesQuery();
            var result = await handler.Handle(query, cancellationToken);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization();


        endpoints.MapPost("/webhooks/stripe", async (
            HttpContext httpContext,
            [FromServices] IStripePaymentService stripePaymentService,
            [FromServices] ProcessStripeWebhookCommandHandler webhookHandler,
            [FromServices] PersonaScript.Server.Observability.Alerts.IOperationalAlertService alertService,
            [FromServices] Microsoft.Extensions.Logging.ILoggerFactory loggerFactory,
            CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("PersonaScript.Server.Endpoints.StripeWebhooks");
            using var reader = new StreamReader(httpContext.Request.Body);
            var rawJson = await reader.ReadToEndAsync(cancellationToken);
            var signatureHeader = httpContext.Request.Headers["Stripe-Signature"].ToString();

            var parseResult = stripePaymentService.ParseWebhookEvent(rawJson, signatureHeader);
            if (parseResult.IsFailure)
            {
                logger.LogError("[Stripe Webhook Error] Falha ao verificar assinatura: {ErrorCode} - {ErrorMessage}",
                    parseResult.Error.Code,
                    parseResult.Error.Message);

                PersonaScript.Server.Observability.Metrics.PersonaScriptMetrics.RecordStripeWebhookFailure("stripe_signature_verification", parseResult.Error.Code);

                await alertService.NotifyPaymentWebhookFailureAsync(
                    new PersonaScript.Server.Observability.Alerts.PaymentWebhookFailureAlert(
                        EventType: "stripe_signature_verification",
                        CustomerId: null,
                        ErrorCode: parseResult.Error.Code,
                        ErrorMessage: parseResult.Error.Message,
                        TimestampUtc: DateTime.UtcNow),
                    cancellationToken);

                return Results.BadRequest(parseResult.Error);
            }

            var processResult = await webhookHandler.Handle(parseResult.Value, cancellationToken);
            if (processResult.IsFailure)
            {
                logger.LogError("[Stripe Webhook Processing Error] Evento {EventType} falhou: {ErrorCode} - {ErrorMessage}",
                    parseResult.Value.EventType,
                    processResult.Error.Code,
                    processResult.Error.Message);

                PersonaScript.Server.Observability.Metrics.PersonaScriptMetrics.RecordStripeWebhookFailure(parseResult.Value.EventType, processResult.Error.Code);

                await alertService.NotifyPaymentWebhookFailureAsync(
                    new PersonaScript.Server.Observability.Alerts.PaymentWebhookFailureAlert(
                        EventType: parseResult.Value.EventType,
                        CustomerId: parseResult.Value.StripeCustomerId,
                        ErrorCode: processResult.Error.Code,
                        ErrorMessage: processResult.Error.Message,
                        TimestampUtc: DateTime.UtcNow),
                    cancellationToken);

                return Results.BadRequest(processResult.Error);
            }

            return Results.Ok(new { status = "success" });
        }).AllowAnonymous().RequireRateLimiting(PersonaScript.Server.Middleware.RateLimitingExtensions.WebhooksPolicy);

        return endpoints;
    }
}

public record CreateCheckoutRequest(
    PlanType PlanType,
    string? SuccessUrl = null,
    string? CancelUrl = null);

public record CreatePortalRequest(
    string? ReturnUrl = null);
