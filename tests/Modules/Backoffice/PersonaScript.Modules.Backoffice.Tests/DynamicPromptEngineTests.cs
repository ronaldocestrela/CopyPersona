using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Modules.Backoffice.Application.Services;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Tests;

public class DynamicPromptEngineTests
{
    private readonly IPromptTemplateRepository _promptRepository;
    private readonly ICouncilRuleRepository _councilRuleRepository;
    private readonly DynamicPromptEngine _engine;

    public DynamicPromptEngineTests()
    {
        _promptRepository = Substitute.For<IPromptTemplateRepository>();
        _councilRuleRepository = Substitute.For<ICouncilRuleRepository>();
        _engine = new DynamicPromptEngine(_promptRepository, _councilRuleRepository);
    }

    [Fact]
    public async Task RenderPromptAsync_ShouldInjectCouncilRules_WhenCouncilKeyIsProvided()
    {
        // Arrange
        var template = PromptTemplate.Create(
            agentName: "Agent1_Persona",
            version: 1,
            systemPrompt: "Instruções do Agente. {{regras_conselho}}",
            userPromptTemplate: "Profissão: {{profissao}}",
            parametersJson: "{}",
            description: "Teste",
            adminEmail: "admin@personascript.ai").Value;

        _promptRepository.GetActiveByAgentNameAsync("Agent1_Persona", Arg.Any<CancellationToken>())
            .Returns(template);

        var cfmRule = CouncilRule.Create("CFM", "Conselho Federal de Medicina", "2.336/2023", "Vedado promessa de resultados.", "Medicina").Value;
        _councilRuleRepository.GetByAcronymAsync("CFM", Arg.Any<CancellationToken>())
            .Returns(cfmRule);

        var vars = new Dictionary<string, string>
        {
            { "conselho", "CFM" },
            { "profissao", "Médico Dermatologista" }
        };

        var fallback = new LLMRequest { SystemPrompt = "Fallback", UserPrompt = "Fallback" };

        // Act
        var result = await _engine.RenderPromptAsync("Agent1_Persona", vars, fallback, CancellationToken.None);

        // Assert
        result.SystemPrompt.Should().Contain("Vedado promessa de resultados.");
        result.UserPrompt.Should().Contain("Médico Dermatologista");
    }
}
