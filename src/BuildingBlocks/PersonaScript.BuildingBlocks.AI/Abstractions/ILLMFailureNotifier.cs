using PersonaScript.BuildingBlocks.AI.Models;

namespace PersonaScript.BuildingBlocks.AI.Abstractions;

public interface ILLMFailureNotifier
{
    Task NotifyFailureAsync(
        LLMProviderType providerType,
        string? errorCode,
        string errorMessage,
        bool allProvidersFailed,
        CancellationToken cancellationToken = default);
}
