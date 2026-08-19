using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Services;

public interface IPersonaPromptBuilder
{
    LLMRequest BuildPrompt(FullAnamneseDto anamnese, string? feedback = null);
}
