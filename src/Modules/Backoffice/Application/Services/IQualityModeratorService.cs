using PersonaScript.Modules.Backoffice.Application.DTOs;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public interface IQualityModeratorService
{
    Task<QualityModerationResultDto> ModerateContentAsync(
        string content,
        string? councilAcronym = null,
        CancellationToken cancellationToken = default);
}
