using FluentAssertions;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using PersonaScript.Modules.Personas.Tests;
using PersonaScript.Modules.Scripts.Application.Services;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class ContentPlanPromptBuilderTests
{
    private readonly ContentPlanPromptBuilder _promptBuilder;

    public ContentPlanPromptBuilderTests()
    {
        _promptBuilder = new ContentPlanPromptBuilder();
    }

    [Fact]
    public void BuildPrompt_ShouldIncludeRotinaAndEscritaReal_WhenPresentInAnamnese()
    {
        // Arrange
        var anamneseDto = TestAnamneseFactory.CreateFullAnamnese();

        var diagnosis = PersonaDiagnosis.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Frase Exemplo Posicionamento",
            "Síntese Exemplo",
            new IdentidadeMarca("Tom Exemplo", "Estilo", "O Sábio", "O Herói"),
            new List<PilarConteudo> { new PilarConteudo("Educação", 100, "Desc", new List<string> { "Tema" }) },
            new MatrizRestricoes(new List<string> { "Politica" }, new List<string> { "Garantia" }, new List<string>(), "Limites")).Value;

        // Act
        var prompt = _promptBuilder.BuildPrompt(anamneseDto, diagnosis);

        // Assert
        prompt.Should().Contain("--- PERFIL DO PROFISSIONAL ---");
        prompt.Should().Contain("Frase Exemplo Posicionamento");
        prompt.Should().Contain("--- DADOS DE ROTINA E CAPACIDADE (ETAPA 9 DA ANAMNESE) ---");
        prompt.Should().Contain("--- AMOSTRA DE ESCRITA REAL (ETAPA 8.2) ---");
        prompt.Should().Contain("--- RESTRIÇÕES E DIRETRIZES ÉTICAS ---");
    }
}
