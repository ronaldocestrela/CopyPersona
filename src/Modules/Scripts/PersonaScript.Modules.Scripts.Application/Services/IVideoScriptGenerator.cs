using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.DTOs;

namespace PersonaScript.Modules.Scripts.Application.Services;

public interface IVideoScriptGenerator
{
    Task<Result<VideoScriptLLMResponseDto>> GenerateAsync(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis,
        string tema,
        string pilarConteudo,
        string objetivo,
        string? tomDesejado = null,
        string? instrucoesAdicionais = null,
        CancellationToken cancellationToken = default);
}
