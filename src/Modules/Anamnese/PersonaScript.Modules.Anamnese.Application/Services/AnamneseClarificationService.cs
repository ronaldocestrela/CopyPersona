using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Services;

public class AnamneseClarificationService : IAnamneseClarificationService
{
    private readonly HeuristicClarificationAnalyzer _analyzer;

    public AnamneseClarificationService(HeuristicClarificationAnalyzer analyzer)
    {
        _analyzer = analyzer;
    }

    public Task<Result<ClarificationAnalysisResultDto>> AnalyzeStepAsync(int stepNumber, object stepDto, CancellationToken cancellationToken = default)
    {
        if (stepDto == null)
        {
            return Task.FromResult(Result.Success(new ClarificationAnalysisResultDto(false, new List<ClarificationItemDto>())));
        }

        var result = _analyzer.AnalyzeStep(stepNumber, stepDto);
        return Task.FromResult(Result.Success(result));
    }
}
