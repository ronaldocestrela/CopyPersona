using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Resilience;

public sealed class FallbackLLMProviderDecorator : ILLMProvider
{
    private readonly ILLMProvider _primaryProvider;
    private readonly IReadOnlyList<ILLMProvider> _fallbackProviders;
    private readonly ILLMJsonParser _jsonParser;
    private readonly ILogger<FallbackLLMProviderDecorator>? _logger;

    public LLMProviderType ProviderType => _primaryProvider.ProviderType;

    public FallbackLLMProviderDecorator(
        ILLMProvider primaryProvider,
        IEnumerable<ILLMProvider> fallbackProviders,
        ILLMJsonParser jsonParser,
        ILogger<FallbackLLMProviderDecorator>? logger = null)
    {
        _primaryProvider = primaryProvider ?? throw new ArgumentNullException(nameof(primaryProvider));
        _fallbackProviders = fallbackProviders?.ToList() ?? new List<ILLMProvider>();
        _jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
        _logger = logger;
    }

    public async Task<Result<LLMResponse>> CompleteAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var providersToTry = new List<ILLMProvider> { _primaryProvider };
        providersToTry.AddRange(_fallbackProviders);

        var errorsSummary = new List<string>();

        foreach (var provider in providersToTry)
        {
            try
            {
                var result = await provider.CompleteAsync(request, cancellationToken);
                if (result.IsSuccess)
                {
                    if (provider != _primaryProvider)
                    {
                        _logger?.LogWarning(
                            "[LLM Fallback Success] Provedor primário {PrimaryProvider} falhou; resposta obtida com sucesso via fallback {FallbackProvider}.",
                            _primaryProvider.ProviderType,
                            provider.ProviderType);
                    }
                    return result;
                }

                errorsSummary.Add($"[{provider.ProviderType}]: {result.Error.Code} - {result.Error.Message}");
                _logger?.LogWarning(
                    "[LLM Provider Failed] Provedor {ProviderType} retornou erro: {ErrorCode} - {ErrorMessage}. Tentando próximo provedor se disponível...",
                    provider.ProviderType,
                    result.Error.Code,
                    result.Error.Message);
            }
            catch (Exception ex)
            {
                errorsSummary.Add($"[{provider.ProviderType} Exception]: {ex.Message}");
                _logger?.LogError(
                    ex,
                    "[LLM Provider Exception] Exceção ao chamar provedor {ProviderType}. Tentando próximo provedor...",
                    provider.ProviderType);
            }
        }

        string aggregatedErrors = string.Join(" | ", errorsSummary);
        return Result.Failure<LLMResponse>(LLMErrors.AllProvidersFailed(aggregatedErrors));
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
        var providersToTry = new List<ILLMProvider> { _primaryProvider };
        providersToTry.AddRange(_fallbackProviders);

        bool streamStarted = false;

        foreach (var provider in providersToTry)
        {
            IAsyncEnumerator<Result<LLMStreamChunk>>? enumerator = null;
            bool providerFailed = false;

            try
            {
                enumerator = provider.CompleteStreamAsync(request, cancellationToken).GetAsyncEnumerator(cancellationToken);

                while (await enumerator.MoveNextAsync())
                {
                    var chunkResult = enumerator.Current;
                    if (chunkResult.IsFailure)
                    {
                        providerFailed = true;
                        break;
                    }

                    streamStarted = true;
                    yield return chunkResult;
                }

                if (!providerFailed && streamStarted)
                {
                    yield break;
                }
            }
            finally
            {
                if (enumerator is not null)
                {
                    await enumerator.DisposeAsync();
                }
            }

            if (streamStarted)
            {
                // Se a transmissão já havia começado e falhou no meio, não podemos fazer fallback limpo
                yield break;
            }
        }

        yield return Result.Failure<LLMStreamChunk>(
            LLMErrors.AllProvidersFailed("Todos os provedores de streaming falharam antes de iniciar a resposta."));
    }
}
