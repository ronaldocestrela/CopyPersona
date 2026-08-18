using FluentAssertions;
using PersonaScript.Modules.Personas.Application.Services;

namespace PersonaScript.Modules.Personas.Tests.Services;

public class PersonaPromptBuilderTests
{
    private readonly PersonaPromptBuilder _builder = new();

    [Fact]
    public void BuildPrompt_WithFullAnamnese_ShouldIncludeAllStagesAndProhibitions()
    {
        // Arrange
        var fullAnamnese = TestAnamneseFactory.CreateFullAnamnese();

        // Act
        var request = _builder.BuildPrompt(fullAnamnese);

        // Assert
        request.Should().NotBeNull();
        request.SystemPrompt.Should().Contain("Estrategista de Persona");
        request.SystemPrompt.Should().Contain("JSON");
        request.UserPrompt.Should().Contain("Dra. Ana Paula Silva");
        request.UserPrompt.Should().Contain("Dermatologia Estética");
        request.UserPrompt.Should().Contain("Dancinhas ridículas e exposição apelativa");
        request.UserPrompt.Should().Contain("Política partidária, religião");
    }
}
