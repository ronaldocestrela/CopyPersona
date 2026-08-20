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
            CancellationToken cancellationToken) =>
        {
            using var reader = new StreamReader(httpContext.Request.Body);
            var rawJson = await reader.ReadToEndAsync(cancellationToken);
            var signatureHeader = httpContext.Request.Headers["Stripe-Signature"].ToString();

            var parseResult = stripePaymentService.ParseWebhookEvent(rawJson, signatureHeader);
            if (parseResult.IsFailure)
            {
                return Results.BadRequest(parseResult.Error);
            }

            var processResult = await webhookHandler.Handle(parseResult.Value, cancellationToken);
            return processResult.IsSuccess ? Results.Ok(new { status = "success" }) : Results.BadRequest(processResult.Error);
        }).AllowAnonymous();

        return endpoints;
    }
}

public record CreateCheckoutRequest(
    PlanType PlanType,
    string? SuccessUrl = null,
    string? CancelUrl = null);

public record CreatePortalRequest(
    string? ReturnUrl = null);
