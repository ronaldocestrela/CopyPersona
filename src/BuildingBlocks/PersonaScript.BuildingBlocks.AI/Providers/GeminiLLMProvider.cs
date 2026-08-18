using System.Net;
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

public sealed class GeminiLLMProvider : ILLMProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILLMJsonParser _jsonParser;
    private readonly string _apiKey;

    public LLMProviderType ProviderType => LLMProviderType.GoogleGemini;

    public GeminiLLMProvider(
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
            return Result.Failure<LLMResponse>(LLMErrors.MissingApiKey("GoogleGemini"));
        }

        string modelName = string.IsNullOrWhiteSpace(request.Model) || request.Model.StartsWith("gpt")
            ? "gemini-1.5-pro"
            : request.Model;

        string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_apiKey}";

        var contents = new List<object>
        {
            new
            {
                role = "user",
                parts = new object[] { new { text = request.UserPrompt } }
            }
        };

        var payload = new Dictionary<string, object>
        {
            ["contents"] = contents,
            ["generationConfig"] = new Dictionary<string, object>
            {
                ["temperature"] = request.Temperature,
                ["maxOutputTokens"] = request.MaxTokens,
                ["responseMimeType"] = request.ResponseFormatJson ? "application/json" : "text/plain"
            }
        };

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            payload["systemInstruction"] = new
            {
                parts = new object[] { new { text = request.SystemPrompt } }
            };
        }

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            using var httpResponse = await _httpClient.PostAsJsonAsync(endpoint, payload, cancellationToken);
            stopwatch.Stop();

            if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("GoogleGemini"));
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                string errContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"Gemini HTTP {(int)httpResponse.StatusCode}: {errContent}"));
            }

            var geminiResponse = await httpResponse.Content.ReadFromJsonAsync<GeminiGenerateContentResponse>(cancellationToken);
            string? text = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable("Gemini retornou resposta sem conteúdo de texto."));
            }

            return Result.Success(new LLMResponse
            {
                Content = text,
                ProviderType = LLMProviderType.GoogleGemini,
                ModelUsed = modelName,
                PromptTokens = geminiResponse?.UsageMetadata?.PromptTokenCount ?? 0,
                CompletionTokens = geminiResponse?.UsageMetadata?.CandidatesTokenCount ?? 0,
                LatencyMs = stopwatch.ElapsedMilliseconds
            });
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result.Failure<LLMResponse>(LLMErrors.Timeout("GoogleGemini"));
        }
        catch (Exception ex)
        {
            return Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable($"Gemini Exceção: {ex.Message}"));
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
            ProviderType = LLMProviderType.GoogleGemini
        });
    }

    private sealed class GeminiGenerateContentResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart>? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiUsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }
    }
}
