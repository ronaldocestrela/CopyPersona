using PersonaScript.Modules.Backoffice.Domain.Enums;

namespace PersonaScript.Modules.Backoffice.Application.DTOs;

public record CouncilRuleDto(
    Guid Id,
    string CouncilAcronym,
    string CouncilName,
    string ResolutionNumber,
    string GuidelinesText,
    string Category,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record ForbiddenTermDto(
    Guid Id,
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public record ModerationViolationDto(
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning,
    int MatchIndex
);

public record QualityModerationResultDto(
    bool IsCompliant,
    int Score,
    IReadOnlyList<ModerationViolationDto> Violations,
    string OriginalContent,
    string SanitizedContent,
    CouncilRuleDto? CouncilRuleApplied
);

public record CreateCouncilRuleRequest(
    string CouncilAcronym,
    string CouncilName,
    string ResolutionNumber,
    string GuidelinesText,
    string Category,
    bool IsActive = true
);

public record UpdateCouncilRuleRequest(
    Guid Id,
    string CouncilName,
    string ResolutionNumber,
    string GuidelinesText,
    string Category,
    bool IsActive
);

public record CreateForbiddenTermRequest(
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning,
    bool IsActive = true
);

public record UpdateForbiddenTermRequest(
    Guid Id,
    string Term,
    string Category,
    ForbiddenTermSeverity Severity,
    string ReplacementSuggestion,
    string Reasoning,
    bool IsActive
);
