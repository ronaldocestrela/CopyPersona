using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification;

public record AnalyzeStepClarificationQuery(
    int StepNumber,
    object StepData
) : IQuery<ClarificationAnalysisResultDto>;
