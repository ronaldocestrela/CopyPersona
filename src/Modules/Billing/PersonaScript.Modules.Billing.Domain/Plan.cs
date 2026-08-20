using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Billing.Domain;

public class Plan : BaseEntity
{
    public PlanType PlanType { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal MonthlyPrice { get; private set; }
    public decimal YearlyPrice { get; private set; }
    public int MaxActivePersonas { get; private set; }
    public int MaxScriptsPerMonth { get; private set; }
    public int MaxAiAnalysesPerMonth { get; private set; }
    public bool IsActive { get; private set; }
    public string? StripePriceId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Plan() { }

    public static Result<Plan> Create(
        PlanType planType,
        string name,
        string description,
        decimal monthlyPrice,
        decimal yearlyPrice,
        int maxActivePersonas,
        int maxScriptsPerMonth,
        int maxAiAnalysesPerMonth,
        string? stripePriceId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Plan>(DomainErrors.Plan.InvalidName);
        }

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            PlanType = planType,
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            MonthlyPrice = monthlyPrice,
            YearlyPrice = yearlyPrice,
            MaxActivePersonas = maxActivePersonas,
            MaxScriptsPerMonth = maxScriptsPerMonth,
            MaxAiAnalysesPerMonth = maxAiAnalysesPerMonth,
            StripePriceId = stripePriceId?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return Result.Success(plan);
    }

    public void SetStripePriceId(string stripePriceId)
    {
        StripePriceId = stripePriceId?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
