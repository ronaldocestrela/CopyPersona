using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Scripts.Application.Services;

public interface IContentPlanPromptBuilder
{
    string BuildPrompt(FullAnamneseDto anamnese, PersonaDiagnosis? diagnosis);
}
