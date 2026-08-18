using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Providers;

public sealed class OpenAILLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILLMJsonParser _jsonParser;
    private readonly string _apiKey;
    private readonly string _endpoint;

    public LLMProviderType ProviderType => LLMProviderType.OpenAI;

    public OpenAILLMProvider(
        HttpClient httpClient,
        ILLMJsonParser jsonParser,
        string apiKey,
        string endpoint = "https://api.openai.com/v1/chat/completions")
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _jsonParser = jsonParser ?? throw new ArgumentNullException(nameof(jsonParser));
        _apiKey = apiKey ?? string.Empty;
        _endpoint = endpoint;
    }

    public async Task<Result<LLMResponse>> CompleteAsync(LLMRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return Result.Failure<LLMResponse>(LLMErrors.MissingApiKey("OpenAI"));
        }

        var messages = new List<object>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }
        messages.Add(new { role = "user", content = request.UserPrompt });

        var payload = new Dictionary<string, object>
        {
            ["model"] = string.IsNullOrWhiteSpace(request.Model) ? "gpt-4o" : request.Model,
            ["messages"] = messages,
            ["temperature"] = request.Temperature,
            ["max_tokens"] = request.MaxTokens
        };

        if (request.ResponseFormatJson)
        {
            payload["response_format"] = new { type = "json_object" };
        }

        try
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = JsonContent.Create(payload)
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
            stopwatch.Stop();

            if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("OpenAI"));
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                string errContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"OpenAI HTTP {(int)httpResponse.StatusCode}: {errContent}"));
            }

            var openAiResponse = await httpResponse.Content.ReadFromJsonAsync<OpenAiChatCompletionResponse>(cancellationToken);
            if (openAiResponse?.Choices is null || openAiResponse.Choices.Count == 0)
            {
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable("OpenAI retornou resposta sem escolhas/conteúdo."));
            }

            string content = openAiResponse.Choices[0].Message.Content ?? string.Empty;

            return Result.Success(new LLMResponse
            {
                Content = content,
                ProviderType = LLMProviderType.OpenAI,
                ModelUsed = openAiResponse.Model ?? request.Model,
                PromptTokens = openAiResponse.Usage?.PromptTokens ?? 0,
                CompletionTokens = openAiResponse.Usage?.CompletionTokens ?? 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<LLMResponse>(LLMErrors.Timeout("OpenAI"));
        }
        catch (Exception ex)
        {
            return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"OpenAI Exceção: {ex.Message}"));
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
            ProviderType = LLMProviderType.OpenAI
        });
    }

    private sealed class OpenAiChatCompletionResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<OpenAiChoice> Choices { get; set; } = new();

        [JsonPropertyName("usage")]
        public OpenAiUsage? Usage { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage Message { get; set; } = new();
    }

    private sealed class OpenAiMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class OpenAiUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
    }
}
