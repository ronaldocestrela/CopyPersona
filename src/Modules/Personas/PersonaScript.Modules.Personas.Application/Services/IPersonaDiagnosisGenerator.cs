using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Services;

public interface IPersonaDiagnosisGenerator
{
    Task<Result<PersonaDiagnosisLLMResponseDto>> GenerateAsync(FullAnamneseDto anamnese, string? feedback = null, CancellationToken cancellationToken = default);
}
