using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Abstractions;

public interface ILLMProvider
{
    LLMProviderType ProviderType { get; }

    Task<Result<LLMResponse>> CompleteAsync(LLMRequest request, CancellationToken cancellationToken = default);

    Task<Result<T>> CompleteStructuredAsync<T>(LLMRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Result<LLMStreamChunk>> CompleteStreamAsync(LLMRequest request, CancellationToken cancellationToken = default);
}
