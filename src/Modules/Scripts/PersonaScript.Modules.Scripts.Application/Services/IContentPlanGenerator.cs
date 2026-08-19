using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.DTOs;

namespace PersonaScript.Modules.Scripts.Application.Services;

public interface IContentPlanGenerator
{
    Task<Result<ContentPlanLLMResponseDto>> GeneratePlanAsync(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis,
        CancellationToken cancellationToken = default);
}
