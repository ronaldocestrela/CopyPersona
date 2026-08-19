using FluentAssertions;
using NSubstitute;
using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Application.Services;

namespace PersonaScript.Modules.Personas.Tests.Services;

public class PersonaDiagnosisGeneratorTests
{
    private readonly ILLMProvider _llmProvider = Substitute.For<ILLMProvider>();
    private readonly IPersonaPromptBuilder _promptBuilder = new PersonaPromptBuilder();
    private readonly PersonaDiagnosisGenerator _generator;

    public PersonaDiagnosisGeneratorTests()
    {
        _generator = new PersonaDiagnosisGenerator(_llmProvider, _promptBuilder);
    }

    [Fact]
    public async Task GenerateAsync_WhenLLMReturnsValidStructuredOutput_ShouldReturnSuccess()
    {
        // Arrange
        var anamnese = TestAnamneseFactory.CreateFullAnamnese();

        var llmResponse = new PersonaDiagnosisLLMResponseDto
        {
            FrasePosicionamento = "A especialista em rejuvenescimento consciente.",
            SintesePerfil = "Profissional referência em dermatologia natural.",
            TomDeVoz = "Empático e Científico",
            EstiloVisualSugerido = "Clean e Elegante",
            ArquetipoPrincipal = "O Sábio",
            ArquetipoSecundario = "O Cuidador",
            PilaresConteudo = new List<PilarLLMItemDto>
            {
                new() { Nome = "Educação", Percentual = 40, Descricao = "Conceitos", ExemplosTopicos = new() { "Topico 1" } },
                new() { Nome = "Prova Social", Percentual = 30, Descricao = "Casos", ExemplosTopicos = new() { "Topico 2" } },
                new() { Nome = "Autoridade", Percentual = 30, Descricao = "Opinião", ExemplosTopicos = new() { "Topico 3" } }
            },
            TemasProibidos = new List<string> { "Sensacionalismo" },
            PalavrasEvitar = new List<string> { "Milagre" },
            DiretrizesInegociaveis = new List<string> { "Ética" },
            LimitesExposicao = "Sem exposição da vida íntima"
        };

        _llmProvider.CompleteStructuredAsync<PersonaDiagnosisLLMResponseDto>(
            Arg.Any<LLMRequest>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Success(llmResponse));

        // Act
        var result = await _generator.GenerateAsync(anamnese, cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FrasePosicionamento.Should().Be("A especialista em rejuvenescimento consciente.");
        result.Value.PilaresConteudo.Sum(p => p.Percentual).Should().Be(100);
    }

    [Fact]
    public async Task GenerateAsync_WhenLLMFails_ShouldFallbackToHeuristicDiagnosis()
    {
        // Arrange
        var anamnese = TestAnamneseFactory.CreateFullAnamnese();

        _llmProvider.CompleteStructuredAsync<PersonaDiagnosisLLMResponseDto>(
            Arg.Any<LLMRequest>(),
            Arg.Any<CancellationToken>()
        ).Returns(Result.Failure<PersonaDiagnosisLLMResponseDto>(new Error("LLM.Unavailable", "Service unavailable")));

        // Act
        var result = await _generator.GenerateAsync(anamnese, cancellationToken: CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FrasePosicionamento.Should().NotBeNullOrEmpty();
        result.Value.PilaresConteudo.Sum(p => p.Percentual).Should().Be(100);
    }
}
