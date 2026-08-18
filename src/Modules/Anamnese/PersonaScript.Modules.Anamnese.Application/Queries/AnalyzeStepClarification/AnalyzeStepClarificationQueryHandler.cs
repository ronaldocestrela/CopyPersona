using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Services;

namespace PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification;

public class AnalyzeStepClarificationQueryHandler : IQueryHandler<AnalyzeStepClarificationQuery, ClarificationAnalysisResultDto>
{
    private readonly IAnamneseClarificationService _clarificationService;

    public AnalyzeStepClarificationQueryHandler(IAnamneseClarificationService clarificationService)
    {
        _clarificationService = clarificationService;
    }

    public async Task<Result<ClarificationAnalysisResultDto>> Handle(AnalyzeStepClarificationQuery query, CancellationToken cancellationToken)
    {
        return await _clarificationService.AnalyzeStepAsync(query.StepNumber, query.StepData, cancellationToken);
    }
}
