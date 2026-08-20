using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class CouncilRule : BaseEntity
{
    public string CouncilAcronym { get; private set; } = string.Empty;
    public string CouncilName { get; private set; } = string.Empty;
    public string ResolutionNumber { get; private set; } = string.Empty;
    public string GuidelinesText { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CouncilRule() { }

    public static Result<CouncilRule> Create(
        string councilAcronym,
        string councilName,
        string resolutionNumber,
        string guidelinesText,
        string category,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(councilAcronym))
        {
            return Result.Failure<CouncilRule>(Error.Validation("CouncilRule.AcronymRequired", "A sigla do conselho é obrigatória."));
        }

        if (string.IsNullOrWhiteSpace(guidelinesText))
        {
            return Result.Failure<CouncilRule>(Error.Validation("CouncilRule.GuidelinesRequired", "O texto das diretrizes éticas é obrigatório."));
        }

        var rule = new CouncilRule
        {
            Id = Guid.NewGuid(),
            CouncilAcronym = councilAcronym.Trim().ToUpperInvariant(),
            CouncilName = string.IsNullOrWhiteSpace(councilName) ? councilAcronym.Trim().ToUpperInvariant() : councilName.Trim(),
            ResolutionNumber = string.IsNullOrWhiteSpace(resolutionNumber) ? "Diretriz Regulatória" : resolutionNumber.Trim(),
            GuidelinesText = guidelinesText.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? "Geral" : category.Trim(),
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(rule);
    }

    public void Update(
        string councilName,
        string resolutionNumber,
        string guidelinesText,
        string category)
    {
        if (!string.IsNullOrWhiteSpace(councilName))
            CouncilName = councilName.Trim();

        if (!string.IsNullOrWhiteSpace(resolutionNumber))
            ResolutionNumber = resolutionNumber.Trim();

        if (!string.IsNullOrWhiteSpace(guidelinesText))
            GuidelinesText = guidelinesText.Trim();

        if (!string.IsNullOrWhiteSpace(category))
            Category = category.Trim();

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
