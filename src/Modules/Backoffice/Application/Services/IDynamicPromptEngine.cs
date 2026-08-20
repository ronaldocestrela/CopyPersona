using PersonaScript.BuildingBlocks.AI.Models;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public interface IDynamicPromptEngine
{
    Task<LLMRequest> RenderPromptAsync(
        string agentName,
        IDictionary<string, string> variables,
        LLMRequest defaultFallbackRequest,
        CancellationToken cancellationToken = default);
}
