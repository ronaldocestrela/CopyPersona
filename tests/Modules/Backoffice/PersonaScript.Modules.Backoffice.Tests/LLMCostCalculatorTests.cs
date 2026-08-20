using PersonaScript.Modules.Backoffice.Application.Services;
using Xunit;

namespace PersonaScript.Modules.Backoffice.Tests;

public class LLMCostCalculatorTests
{
    [Theory]
    [InlineData("gpt-4o", 1000, 1000, 0.0125)] // (1000 * 2.50 / 1M) + (1000 * 10.00 / 1M) = 0.0025 + 0.0100 = 0.0125
    [InlineData("gpt-4o-mini", 1000000, 1000000, 0.75)] // 0.15 + 0.60 = 0.75
    [InlineData("claude-3-5-sonnet", 1000000, 1000000, 18.00)] // 3.00 + 15.00 = 18.00
    [InlineData("gemini-1.5-pro", 1000000, 1000000, 6.25)] // 1.25 + 5.00 = 6.25
    [InlineData("mock", 5000, 5000, 0.0)]
    [InlineData("unknown-model", 1000, 1000, 0.0)]
    public void CalculateCost_ShouldReturnCorrectCostInUSD(string model, int promptTokens, int completionTokens, decimal expectedCost)
    {
        var calculator = new LLMCostCalculator();

        var cost = calculator.CalculateCost(model, promptTokens, completionTokens);

        Assert.Equal(expectedCost, cost);
    }
}
