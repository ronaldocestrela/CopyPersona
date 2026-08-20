using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Application.Services;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Compliance;

public record ModerateContentCommand(
    string Content,
    string? CouncilAcronym = null
) : ICommand<QualityModerationResultDto>;

public sealed class ModerateContentCommandHandler : ICommandHandler<ModerateContentCommand, QualityModerationResultDto>
{
    private readonly IQualityModeratorService _moderatorService;

    public ModerateContentCommandHandler(IQualityModeratorService moderatorService)
    {
        _moderatorService = moderatorService;
    }

    public async Task<Result<QualityModerationResultDto>> Handle(ModerateContentCommand command, CancellationToken cancellationToken)
    {
        var result = await _moderatorService.ModerateContentAsync(
            command.Content,
            command.CouncilAcronym,
            cancellationToken);

        return Result.Success(result);
    }
}
