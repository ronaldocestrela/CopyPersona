using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Scripts.Application.Services;

public interface IVideoScriptPromptBuilder
{
    string BuildPrompt(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis,
        string tema,
        string pilarConteudo,
        string objetivo,
        string? tomDesejado = null,
        string? instrucoesAdicionais = null);
}
