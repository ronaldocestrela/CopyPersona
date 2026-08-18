using FluentAssertions;
using PersonaScript.BuildingBlocks.AI.Errors;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.AI.Providers;
using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.UnitTests;

public class MockLLMProviderTests
{
    private readonly MockLLMProvider _provider;

    public MockLLMProviderTests()
    {
        _provider = new MockLLMProvider(new LLMJsonParser());
    }

    [Fact]
    public async Task CompleteAsync_DefaultBehavior_ReturnsSuccessWithMockContent()
    {
        // Arrange
        var request = new LLMRequest { UserPrompt = "Gere uma bio para Instagram" };

        // Act
        var result = await _provider.CompleteAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Contain("Gere uma bio para Instagram");
        result.Value.ProviderType.Should().Be(LLMProviderType.Mock);
    }

    [Fact]
    public async Task CompleteAsync_CustomEvaluator_ReturnsConfiguredResult()
    {
        // Arrange
        _provider.SetResponseEvaluator(req => Result.Failure<LLMResponse>(LLMErrors.RateLimitExceeded("Mock")));
        var request = new LLMRequest { UserPrompt = "Qualquer prompt" };

        // Act
        var result = await _provider.CompleteAsync(request);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.RateLimitExceeded");
    }

    private sealed record TestObject(string Output);

    [Fact]
    public async Task CompleteStructuredAsync_ValidJsonMock_ReturnsParsedObject()
    {
        // Arrange
        _provider.SetResponseEvaluator(req => Result.Success(new LLMResponse
        {
            Content = """{"Output": "Resultado com sucesso"}""",
            ProviderType = LLMProviderType.Mock,
            ModelUsed = "mock-v1"
        }));

        var request = new LLMRequest { UserPrompt = "Retorne JSON" };

        // Act
        var result = await _provider.CompleteStructuredAsync<TestObject>(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Output.Should().Be("Resultado com sucesso");
    }

    [Fact]
    public async Task CompleteStreamAsync_EmitsChunksSequentially()
    {
        // Arrange
        _provider.SetResponseEvaluator(req => Result.Success(new LLMResponse
        {
            Content = "Primeiro segundo terceiro",
            ProviderType = LLMProviderType.Mock,
            ModelUsed = "mock-v1"
        }));

        var request = new LLMRequest { UserPrompt = "Stream test" };
        var chunks = new List<LLMStreamChunk>();

        // Act
        await foreach (var chunkResult in _provider.CompleteStreamAsync(request))
        {
            chunkResult.IsSuccess.Should().BeTrue();
            chunks.Add(chunkResult.Value);
        }

        // Assert
        chunks.Should().HaveCount(3);
        chunks[0].Delta.Should().Be("Primeiro ");
        chunks[1].Delta.Should().Be("segundo ");
        chunks[2].Delta.Should().Be("terceiro");
        chunks[2].IsCompleted.Should().BeTrue();
    }
}
