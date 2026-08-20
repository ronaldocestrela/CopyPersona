using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Billing.Application.DTOs;

public record PlanDto(
    Guid Id,
    PlanType PlanType,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxActivePersonas,
    int MaxScriptsPerMonth,
    int MaxAiAnalysesPerMonth);

public record SubscriptionSummaryDto(
    Guid Id,
    Guid TenantId,
    Guid PlanId,
    string PlanName,
    SubscriptionStatus Status,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    string? StripeCustomerId,
    string? StripeSubscriptionId);

public record TenantQuotaUsageDto(
    Guid QuotaId,
    Guid TenantId,
    Guid SubscriptionId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int ScriptsGeneratedCount,
    int ScriptsLimit,
    int ActivePersonasCount,
    int ActivePersonasLimit,
    int AiAnalysesCount,
    int AiAnalysesLimit,
    DateTime LastResetAt);

public record CheckoutSessionDto(
    string SessionId,
    string CheckoutUrl);

public record CustomerPortalDto(
    string PortalUrl);

public record SubscriptionDetailsDto(
    Guid SubscriptionId,
    Guid PlanId,
    PlanType PlanType,
    string PlanName,
    decimal MonthlyPrice,
    SubscriptionStatus Status,
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    bool CancelAtPeriodEnd,
    string? StripeCustomerId,
    string? StripeSubscriptionId,
    int ScriptsGeneratedCount,
    int ScriptsLimit,
    int ActivePersonasCount,
    int ActivePersonasLimit,
    int AiAnalysesCount,
    int AiAnalysesLimit,
    DateTime LastQuotaResetAt,
    List<PlanDto> AvailablePlans);

public record InvoiceDto(
    string InvoiceId,
    decimal AmountPaid,
    string Currency,
    string Status,
    string? InvoicePdfUrl,
    DateTime CreatedAt);


