using FluentAssertions;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.AI.Providers;
using PersonaScript.BuildingBlocks.AI.Resilience;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.UnitTests;

public class FallbackLLMProviderDecoratorTests
{
    private readonly LLMJsonParser _jsonParser = new();

    [Fact]
    public async Task CompleteAsync_PrimarySucceeds_ReturnsPrimaryResponseWithoutFallback()
    {
        // Arrange
        var primaryMock = new MockLLMProvider(_jsonParser);
        primaryMock.SetResponseEvaluator(req => Result.Success(new LLMResponse
        {
            Content = "Resposta do Primário",
            ProviderType = LLMProviderType.OpenAI,
            ModelUsed = "gpt-4o"
        }));

        var fallbackMock = new MockLLMProvider(_jsonParser);
        fallbackMock.SetResponseEvaluator(req => Result.Success(new LLMResponse
        {
            Content = "Resposta do Fallback",
            ProviderType = LLMProviderType.GoogleGemini,
            ModelUsed = "gemini-1.5-pro"
        }));

        var decorator = new FallbackLLMProviderDecorator(primaryMock, new[] { fallbackMock }, _jsonParser);
        var request = new LLMRequest { UserPrompt = "Teste" };

        // Act
        var result = await decorator.CompleteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Resposta do Primário");
        result.Value.ProviderType.Should().Be(LLMProviderType.OpenAI);
    }

    [Fact]
    public async Task CompleteAsync_PrimaryFailsWithRateLimit_FallbackSucceeds()
    {
        // Arrange
        var primaryMock = new MockLLMProvider(_jsonParser);
        primaryMock.SetResponseEvaluator(req => Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("OpenAI")));

        var fallbackMock = new MockLLMProvider(_jsonParser);
        fallbackMock.SetResponseEvaluator(req => Result.Success(new LLMResponse
        {
            Content = "Resposta recuperada via Gemini",
            ProviderType = LLMProviderType.GoogleGemini,
            ModelUsed = "gemini-1.5-pro"
        }));

        var decorator = new FallbackLLMProviderDecorator(primaryMock, new[] { fallbackMock }, _jsonParser);
        var request = new LLMRequest { UserPrompt = "Teste Fallback" };

        // Act
        var result = await decorator.CompleteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Resposta recuperada via Gemini");
        result.Value.ProviderType.Should().Be(LLMProviderType.GoogleGemini);
    }

    [Fact]
    public async Task CompleteAsync_AllProvidersFail_ReturnsAllProvidersFailedError()
    {
        // Arrange
        var primaryMock = new MockLLMProvider(_jsonParser);
        primaryMock.SetResponseEvaluator(req => Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("OpenAI")));

        var fallbackMock = new MockLLMProvider(_jsonParser);
        fallbackMock.SetResponseEvaluator(req => Result.Failure<LLMResponse>(LLMErrors.ProviderUnavailable("Gemini")));

        var decorator = new FallbackLLMProviderDecorator(primaryMock, new[] { fallbackMock }, _jsonParser);
        var request = new LLMRequest { UserPrompt = "Teste Falha Total" };

        // Act
        var result = await decorator.CompleteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.AllProvidersFailed");
        result.Error.Message.Should().Contain("RateLimitExceeded").And.Contain("ProviderUnavailable");
    }
}
