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

