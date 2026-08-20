namespace PersonaScript.Modules.Backoffice.Application.Services;

public interface ILLMCostCalculator
{
    decimal CalculateCost(string model, int promptTokens, int completionTokens);
}

public sealed class LLMCostCalculator : ILLMCostCalculator
{
    // Rates per 1,000,000 tokens (USD)
    private static readonly Dictionary<string, (decimal PromptRate, decimal CompletionRate)> ModelRates = new(StringComparer.OrdinalIgnoreCase)
    {
        { "gpt-4o", (2.50m, 10.00m) },
        { "gpt-4o-mini", (0.15m, 0.60m) },
        { "claude-3-5-sonnet", (3.00m, 15.00m) },
        { "gemini-1.5-pro", (1.25m, 5.00m) },
        { "mock", (0.00m, 0.00m) }
    };

    public decimal CalculateCost(string model, int promptTokens, int completionTokens)
    {
        if (string.IsNullOrWhiteSpace(model))
            return 0m;

        if (!ModelRates.TryGetValue(model.Trim(), out var rates))
        {
            // Default rate for unknown models
            return 0m;
        }

        var promptCost = (promptTokens / 1_000_000m) * rates.PromptRate;
        var completionCost = (completionTokens / 1_000_000m) * rates.CompletionRate;

        return Math.Round(promptCost + completionCost, 6);
    }
}
