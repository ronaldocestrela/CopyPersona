using System.Runtime.CompilerServices;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Providers;

public sealed class MockLLMProvider : ILLMProvider
{
    private readonly ILLMJsonParser _jsonParser;
    private Func<LLMRequest, Result<LLMResponse>>? _responseEvaluator;

    public LLMProviderType ProviderType => LLMProviderType.Mock;

    public MockLLMProvider(ILLMJsonParser jsonParser)
    {
        _jsonParser = jsonParser;
    }

    public void SetResponseEvaluator(Func<LLMRequest, Result<LLMResponse>> evaluator)
    {
        _responseEvaluator = evaluator;
    }

    public Task<Result<LLMResponse>> CompleteAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (_responseEvaluator is not null)
        {
            return Task.FromResult(_responseEvaluator(request));
        }

        var defaultResponse = new LLMResponse
        {
            Content = $"[Mock Response for prompt: {request.UserPrompt}]",
            ProviderType = LLMProviderType.Mock,
            ModelUsed = request.Model,
            PromptTokens = 10,
            CompletionTokens = 20,
            LatencyMs = 50
        };

        return Task.FromResult(Result.Success(defaultResponse));
    }

    public async Task<Result<T>> CompleteStructuredAsync<T>(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var responseResult = await CompleteAsync(request, cancellationToken);
        if (responseResult.IsFailure)
        {
            return Result.Failure<T>(responseResult.Error);
        }

        return _jsonParser.Parse<T>(responseResult.Value.Content);
    }

    public async IAsyncEnumerable<Result<LLMStreamChunk>> CompleteStreamAsync(
        LLMRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var responseResult = await CompleteAsync(request, cancellationToken);
        if (responseResult.IsFailure)
        {
            yield return Result.Failure<LLMStreamChunk>(responseResult.Error);
            yield break;
        }

        string content = responseResult.Value.Content;
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            string chunkText = (i == words.Length - 1) ? words[i] : words[i] + " ";
            yield return Result.Success(new LLMStreamChunk
            {
                Delta = chunkText,
                IsCompleted = (i == words.Length - 1),
                ProviderType = LLMProviderType.Mock
            });
        }
    }
}
