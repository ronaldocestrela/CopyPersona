using System.Text.RegularExpressions;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Enums;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public sealed class QualityModeratorService : IQualityModeratorService
{
    private readonly IForbiddenTermRepository _forbiddenTermRepository;
    private readonly ICouncilRuleRepository _councilRuleRepository;

    public QualityModeratorService(
        IForbiddenTermRepository forbiddenTermRepository,
        ICouncilRuleRepository councilRuleRepository)
    {
        _forbiddenTermRepository = forbiddenTermRepository;
        _councilRuleRepository = councilRuleRepository;
    }

    public async Task<QualityModerationResultDto> ModerateContentAsync(
        string content,
        string? councilAcronym = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return new QualityModerationResultDto(
                IsCompliant: true,
                Score: 100,
                Violations: Array.Empty<ModerationViolationDto>(),
                OriginalContent: string.Empty,
                SanitizedContent: string.Empty,
                CouncilRuleApplied: null);
        }

        var activeTerms = await _forbiddenTermRepository.GetAllActiveAsync(cancellationToken);
        CouncilRuleDto? councilRuleDto = null;

        if (!string.IsNullOrWhiteSpace(councilAcronym))
        {
            var rule = await _councilRuleRepository.GetByAcronymAsync(councilAcronym, cancellationToken);
            if (rule != null)
            {
                councilRuleDto = new CouncilRuleDto(
                    rule.Id,
                    rule.CouncilAcronym,
                    rule.CouncilName,
                    rule.ResolutionNumber,
                    rule.GuidelinesText,
                    rule.Category,
                    rule.IsActive,
                    rule.CreatedAt,
                    rule.UpdatedAt);
            }
        }

        var violations = new List<ModerationViolationDto>();
        var sanitizedContent = content;

        foreach (var termItem in activeTerms)
        {
            var regexPattern = @"\b" + Regex.Escape(termItem.Term) + @"\b";
            var matches = Regex.Matches(content, regexPattern, RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                violations.Add(new ModerationViolationDto(
                    Term: termItem.Term,
                    Category: termItem.Category,
                    Severity: termItem.Severity,
                    ReplacementSuggestion: termItem.ReplacementSuggestion,
                    Reasoning: termItem.Reasoning,
                    MatchIndex: match.Index));
            }

            if (matches.Count > 0 && !string.IsNullOrWhiteSpace(termItem.ReplacementSuggestion))
            {
                sanitizedContent = Regex.Replace(
                    sanitizedContent,
                    regexPattern,
                    termItem.ReplacementSuggestion,
                    RegexOptions.IgnoreCase);
            }
        }

        int scorePenalty = 0;
        foreach (var v in violations)
        {
            scorePenalty += v.Severity == ForbiddenTermSeverity.Prohibited ? 35 : 15;
        }

        int finalScore = Math.Max(0, 100 - scorePenalty);
        bool isCompliant = violations.Count(v => v.Severity == ForbiddenTermSeverity.Prohibited) == 0;

        return new QualityModerationResultDto(
            IsCompliant: isCompliant,
            Score: finalScore,
            Violations: violations.OrderBy(v => v.MatchIndex).ToList(),
            OriginalContent: content,
            SanitizedContent: sanitizedContent,
            CouncilRuleApplied: councilRuleDto);
    }
}
