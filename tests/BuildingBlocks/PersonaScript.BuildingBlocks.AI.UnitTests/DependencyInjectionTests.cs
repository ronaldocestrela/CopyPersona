using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.AI.Parsing;
using PersonaScript.BuildingBlocks.AI.Resilience;

namespace PersonaScript.BuildingBlocks.AI.UnitTests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddAIBuildingBlock_RegistersILLMProviderWithFallbackDecorator()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["LLM:PrimaryProvider"] = "Mock",
            ["LLM:FallbackProviders:0"] = "Mock"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddAIBuildingBlock(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var jsonParser = provider.GetService<ILLMJsonParser>();
        jsonParser.Should().NotBeNull();

        var llmProvider = provider.GetService<ILLMProvider>();
        llmProvider.Should().NotBeNull();
        llmProvider.Should().BeOfType<FallbackLLMProviderDecorator>();
        llmProvider!.ProviderType.Should().Be(LLMProviderType.Mock);
    }
}
