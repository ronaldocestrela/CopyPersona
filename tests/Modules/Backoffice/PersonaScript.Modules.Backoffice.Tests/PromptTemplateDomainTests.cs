using FluentAssertions;
using PersonaScript.Modules.Backoffice.Domain;

namespace PersonaScript.Modules.Backoffice.Tests;

public class PromptTemplateDomainTests
{
    [Fact]
    public void Create_ShouldReturnSuccess_WhenParametersAreValid()
    {
        var result = PromptTemplate.Create(
            agentName: "Agent1_Diagnosis",
            version: 1,
            systemPrompt: "Você é o Agente 1...",
            userPromptTemplate: "Ficha de Anamnese: {{AnamneseData}}",
            parametersJson: "{\"Temperature\": 0.5}",
            description: "Versão inicial",
            adminEmail: "admin@personascript.ai",
            isActive: true);

        result.IsSuccess.Should().BeTrue();
        var template = result.Value;
        template.AgentName.Should().Be("Agent1_Diagnosis");
        template.Version.Should().Be(1);
        template.SystemPrompt.Should().Be("Você é o Agente 1...");
        template.UserPromptTemplate.Should().Be("Ficha de Anamnese: {{AnamneseData}}");
        template.ParametersJson.Should().Be("{\"Temperature\": 0.5}");
        template.Description.Should().Be("Versão inicial");
        template.CreatedByAdminEmail.Should().Be("admin@personascript.ai");
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldFail_WhenAgentNameIsEmpty()
    {
        var result = PromptTemplate.Create(
            agentName: "",
            version: 1,
            systemPrompt: "System prompt",
            userPromptTemplate: "User prompt",
            parametersJson: "{}",
            description: "Descrição",
            adminEmail: "admin@personascript.ai");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PromptTemplate.AgentNameRequired");
    }

    [Fact]
    public void ActivateAndDeactivate_ShouldToggleIsActiveStatus()
    {
        var template = PromptTemplate.Create(
            agentName: "Agent2_VideoScript",
            version: 1,
            systemPrompt: "System",
            userPromptTemplate: "User",
            parametersJson: "{}",
            description: "Desc",
            adminEmail: "admin@personascript.ai",
            isActive: false).Value;

        template.IsActive.Should().BeFalse();

        template.Activate();
        template.IsActive.Should().BeTrue();

        template.Deactivate();
        template.IsActive.Should().BeFalse();
    }
}
