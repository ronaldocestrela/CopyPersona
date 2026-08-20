using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Domain;

public sealed class ForbiddenTerm : BaseEntity
{
    public string Term { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public ForbiddenTermSeverity Severity { get; private set; }
    public string ReplacementSuggestion { get; private set; } = string.Empty;
    public string Reasoning { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ForbiddenTerm() { }

    public static Result<ForbiddenTerm> Create(
        string term,
        string category,
        ForbiddenTermSeverity severity,
        string replacementSuggestion,
        string reasoning,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return Result.Failure<ForbiddenTerm>(Error.Validation("ForbiddenTerm.TermRequired", "O termo ou expressão proibida é obrigatório."));
        }

        var item = new ForbiddenTerm
        {
            Id = Guid.NewGuid(),
            Term = term.Trim(),
            Category = string.IsNullOrWhiteSpace(category) ? "Geral" : category.Trim(),
            Severity = severity,
            ReplacementSuggestion = string.IsNullOrWhiteSpace(replacementSuggestion) ? string.Empty : replacementSuggestion.Trim(),
            Reasoning = string.IsNullOrWhiteSpace(reasoning) ? "Restrição regulatória ou diretriz de publicidade." : reasoning.Trim(),
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return Result.Success(item);
    }

    public void Update(
        string term,
        string category,
        ForbiddenTermSeverity severity,
        string replacementSuggestion,
        string reasoning)
    {
        if (!string.IsNullOrWhiteSpace(term))
            Term = term.Trim();

        if (!string.IsNullOrWhiteSpace(category))
            Category = category.Trim();

        Severity = severity;
        ReplacementSuggestion = replacementSuggestion?.Trim() ?? string.Empty;
        Reasoning = reasoning?.Trim() ?? string.Empty;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
