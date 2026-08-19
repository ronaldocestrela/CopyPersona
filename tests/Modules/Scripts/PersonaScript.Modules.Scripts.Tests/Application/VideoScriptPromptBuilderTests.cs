using FluentAssertions;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;
using PersonaScript.Modules.Personas.Tests;
using PersonaScript.Modules.Scripts.Application.Services;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Application;

public class VideoScriptPromptBuilderTests
{
    [Fact]
    public void BuildPrompt_ShouldIncludeEscritaReal82_AndRestricoesInegociaveis()
    {
        // Arrange
        var builder = new VideoScriptPromptBuilder();
        var anamneseDto = TestAnamneseFactory.CreateFullAnamnese();
        var diagnosis = CreateTestPersonaDiagnosis();

        // Act
        var prompt = builder.BuildPrompt(
            anamneseDto,
            diagnosis,
            tema: "Como organizar seu planejamento financeiro pessoal",
            pilarConteudo: "Educação & Esclarecimento",
            objetivo: "Gerar engajamento e comentários",
            tomDesejado: "Empático e Didático",
            instrucoesAdicionais: "Focar em jovens profissionais");

        // Assert
        prompt.Should().NotBeNullOrEmpty();
        // 1. Clonagem de tom de voz humano (Amostra 8.2)
        prompt.Should().Contain("Exemplo de escrita real do profissional");
        prompt.Should().Contain("Estude o estilo de escrita acima para clonar a cadência e o tom de voz");

        // 2. Restrições inegociáveis (5.3, 6.1, 8.4, 6.6)
        prompt.Should().Contain("Dancinhas ridículas e exposição apelativa");
        prompt.Should().Contain("Política partidária, religião");
        prompt.Should().Contain("Seguir resoluções do CFM/SBD");

        // 3. Os 3 blocos obrigatórios
        prompt.Should().Contain("Gancho (Hook)");
        prompt.Should().Contain("Retenção (Body)");
        prompt.Should().Contain("Chamada para Ação (CTA)");

        // 4. Tema e Pilar
        prompt.Should().Contain("Como organizar seu planejamento financeiro pessoal");
        prompt.Should().Contain("Educação & Esclarecimento");
    }

    private static PersonaDiagnosis CreateTestPersonaDiagnosis()
    {
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();

        var identidade = new IdentidadeMarca("Empático e Direto", "Clean", "O Sábio", "O Herói");
        var pilares = new List<PilarConteudo>
        {
            new PilarConteudo("Educação & Esclarecimento", 100, "Explicar conceitos", new List<string> { "Dicas" })
        };
        var restricoes = new MatrizRestricoes(
            new List<string> { "Política" },
            new List<string> { "Garantias milagrosas" },
            new List<string> { "Respeito ao cliente" },
            "Limites de exposição"
        );

        return PersonaDiagnosis.Create(
            tenantId,
            anamneseId,
            "Referência em finanças pessoais para autônomos",
            "Síntese do perfil",
            identidade,
            pilares,
            restricoes).Value;
    }
}
