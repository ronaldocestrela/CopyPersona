using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Services;

public interface IAnamneseClarificationService
{
    Task<Result<ClarificationAnalysisResultDto>> AnalyzeStepAsync(int stepNumber, object stepDto, CancellationToken cancellationToken = default);
}
