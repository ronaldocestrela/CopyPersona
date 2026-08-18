using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Providers;

public sealed class AnthropicLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILLMJsonParser _jsonParser;
    private readonly string _apiKey;

    public LLMProviderType ProviderType => LLMProviderType.Anthropic;

    public AnthropicLLMProvider(
        HttpClient httpClient,
        ILLMJsonParser jsonParser,
        string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
        _apiKey = apiKey ?? string.Empty;
    }

    public async Task<Result<LLMResponse>> CompleteAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return Result.Failure<LLMResponse>(LLMErrors.MissingApiKey("Anthropic"));
        }

        string modelName = string.IsNullOrWhiteSpace(request.Model) || request.Model.StartsWith("gpt")
            ? "claude-3-5-sonnet-20241022"
            : request.Model;

        const string endpoint = "https://api.anthropic.com/v1/messages";

        var payload = new Dictionary<string, object>
        {
            ["model"] = modelName,
            ["max_tokens"] = request.MaxTokens,
            ["temperature"] = request.Temperature,
            ["messages"] = new object[]
            {
                new { role = "user", content = request.UserPrompt }
            }
        };

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            payload["system"] = request.SystemPrompt;
        }

        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            httpRequest.Headers.Add("x-api-key", _apiKey);
            httpRequest.Headers.Add("anthropic-version", "2023-06-01");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            stopwatch.Stop();

            if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("Anthropic"));
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                string errContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"Anthropic HTTP {(int)httpResponse.StatusCode}: {errContent}"));
            }

            var anthropicResponse = await httpResponse.Content.ReadFromJsonAsync<AnthropicMessagesResponse>(cancellationToken);
            string? text = anthropicResponse?.Content?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable("Anthropic retornou resposta sem bloco de texto."));
            }

            return Result.Success(new LLMResponse
            {
                Content = text,
                ProviderType = LLMProviderType.Anthropic,
                ModelUsed = modelName,
                PromptTokens = anthropicResponse?.Usage?.InputTokens ?? 0,
                CompletionTokens = anthropicResponse?.Usage?.OutputTokens ?? 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<LLMResponse>(LLMErrors.Timeout("Anthropic"));
        }
        catch (Exception ex)
        {
            return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"Anthropic Exceção: {ex.Message}"));
        }
    }

    public async Task<Result<T>> CompleteStructuredAsync<T>(LLMRequest request, CancellationToken cancellationToken = default)
    {
        var jsonRequest = request with { ResponseFormatJson = true };
        var responseResult = await CompleteAsync(jsonRequest, cancellationToken);

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
        var response = await CompleteAsync(request, cancellationToken);
        if (response.IsFailure)
        {
            yield return Result.Failure<LLMStreamChunk>(response.Error);
            yield break;
        }

        yield return Result.Success(new LLMStreamChunk
        {
            Delta = response.Value.Content,
            IsCompleted = true,
            ProviderType = LLMProviderType.Anthropic
        });
    }

    private sealed class AnthropicMessagesResponse
    {
        [JsonPropertyName("content")]
        public List<AnthropicContentBlock>? Content { get; set; }

        [JsonPropertyName("usage")]
        public AnthropicUsage? Usage { get; set; }
    }

    private sealed class AnthropicContentBlock
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class AnthropicUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
