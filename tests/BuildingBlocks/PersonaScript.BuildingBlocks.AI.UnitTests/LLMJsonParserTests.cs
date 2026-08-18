using FluentAssertions;
using PersonaScript.BuildingBlocks.AI.Parsing;

namespace PersonaScript.BuildingBlocks.AI.UnitTests;

public class LLMJsonParserTests
{
    private readonly LLMJsonParser _parser = new();

    private sealed record TestPersona(string Nome, string Nicho, int AnosExperiencia);

    [Fact]
    public void Parse_ValidJson_ReturnsSuccess()
    {
        // Arrange
        string json = """{"Nome": "Dra. Maria", "Nicho": "Harmonização Facial", "AnosExperiencia": 8}""";

        // Act
        var result = _parser.Parse<TestPersona>(json);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nome.Should().Be("Dra. Maria");
        result.Value.Nicho.Should().Be("Harmonização Facial");
        result.Value.AnosExperiencia.Should().Be(8);
    }

    [Fact]
    public void Parse_JsonInsideMarkdownCodeBlock_ReturnsSuccess()
    {
        // Arrange
        string rawMarkdown = """
            Aqui está o perfil solicitado em formato JSON:
            ```json
            {
                "Nome": "Dr. Carlos",
                "Nicho": "Implantodontia",
                "AnosExperiencia": 12
            }
            ```
            Espero que ajude!
            """;

        // Act
        var result = _parser.Parse<TestPersona>(rawMarkdown);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Nome.Should().Be("Dr. Carlos");
        result.Value.Nicho.Should().Be("Implantodontia");
        result.Value.AnosExperiencia.Should().Be(12);
    }

    [Fact]
    public void Parse_InvalidJson_ReturnsFailureWithInvalidJsonResponseError()
    {
        // Arrange
        string invalidJson = "{ Nome: 'Dr. Carlos' (sintaxe quebrada) }";

        // Act
        var result = _parser.Parse<TestPersona>(invalidJson);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.InvalidJsonResponse");
    }

    [Fact]
    public void Parse_NullOrEmptyString_ReturnsFailure()
    {
        // Act
        var result = _parser.Parse<TestPersona>("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("AI.InvalidJsonResponse");
    }
}
