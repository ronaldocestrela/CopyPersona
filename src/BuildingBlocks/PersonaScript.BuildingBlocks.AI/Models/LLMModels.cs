namespace PersonaScript.BuildingBlocks.AI.Models;

public enum LLMProviderType
{
    OpenAI = 1,
    AzureOpenAI = 2,
    GoogleGemini = 3,
    Anthropic = 4,
    Mock = 99
}

public sealed record LLMRequest
{
    public required string UserPrompt { get; init; }
    public string? SystemPrompt { get; init; }
    public string Model { get; init; } = "gpt-4o";
    public double Temperature { get; init; } = 0.7;
    public int MaxTokens { get; init; } = 2048;
    public bool ResponseFormatJson { get; init; } = false;
    public IDictionary<string, object>? ExtraParameters { get; init; }
}

public sealed record LLMResponse
{
    public required string Content { get; init; }
    public required LLMProviderType ProviderType { get; init; }
    public required string ModelUsed { get; init; }
    public int PromptTokens { get; init; }
    public int CompletionTokens { get; init; }
    public int TotalTokens => PromptTokens + CompletionTokens;
    public long LatencyMs { get; init; }
}

public sealed record LLMStreamChunk
{
    public required string Delta { get; init; }
    public bool IsCompleted { get; init; }
    public LLMProviderType ProviderType { get; init; }
}
