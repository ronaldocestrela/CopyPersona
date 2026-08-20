using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public record PlanDto(
    Guid Id,
    PlanType PlanType,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxActivePersonas,
    int MaxScriptsPerMonth,
    int MaxAiAnalysesPerMonth,
    bool IsActive,
    string? StripePriceId);
