using PersonaScript.BuildingBlocks.AI.Models;

namespace PersonaScript.BuildingBlocks.AI.Configuration;

public sealed class LLMOptions
{
    public const string SectionName = "LLM";

    public LLMProviderType PrimaryProvider { get; set; } = LLMProviderType.Mock;

    public List<LLMProviderType> FallbackProviders { get; set; } = new()
    {
        LLMProviderType.Mock
    };

    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiEndpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string DefaultOpenAiModel { get; set; } = "gpt-4o";

    public string GeminiApiKey { get; set; } = string.Empty;
    public string DefaultGeminiModel { get; set; } = "gemini-1.5-pro";

    public string AnthropicApiKey { get; set; } = string.Empty;
    public string DefaultAnthropicModel { get; set; } = "claude-3-5-sonnet-20241022";

    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
}
