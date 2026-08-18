using FluentAssertions;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;

namespace PersonaScript.Modules.Personas.Tests.Domain;

public class PersonaDiagnosisTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();
        var frasePosicionamento = "O mentor estratégico dos médicos que buscam posicionamento premium.";
        var sintesePerfil = "Profissional altamente qualificado com foco em atendimento humanizado.";

        var identidade = new IdentidadeMarca(
            TomDeVoz: "Empático, Autoritário e Inspirador",
            EstiloVisualSugerido: "Minimalista elegante com tons frios",
            ArquetipoPrincipal: "O Sábio",
            ArquetipoSecundario: "O Herói"
        );

        var pilares = new List<PilarConteudo>
        {
            new PilarConteudo("Educação & Conceitos", 30, "Explicação clara de procedimentos", new[] { "Mitos e verdades", "Como funciona" }),
            new PilarConteudo("Prova & Bastidores", 25, "Estudos de caso e dia a dia", new[] { "Bastidores da clínica", "Resultados de pacientes" }),
            new PilarConteudo("Autoridade & Opinião", 25, "Visão crítica sobre o mercado", new[] { "Análise de novidades", "Erros comuns" }),
            new PilarConteudo("Conexão & Estilo de Vida", 20, "Valores pessoais e ética", new[] { "Minha rotina", "Por que escolhi a profissão" })
        };

        var restricoes = new MatrizRestricoes(
            TemasProibidos: new[] { "Política partidária", "Dancinhas de mídias sociais" },
            PalavrasEvitar: new[] { "Barato", "Desconto", "Garantido" },
            DiretrizesInegociaveis: new[] { "Seguir estritamente o código de ética", "Sem sensacionalismo" },
            LimitesExposicao: "Não expor familiares ou endereço residencial"
        );

        // Act
        var result = PersonaDiagnosis.Create(
            tenantId,
            anamneseId,
            frasePosicionamento,
            sintesePerfil,
            identidade,
            pilares,
            restricoes
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.AnamneseId.Should().Be(anamneseId);
        result.Value.FrasePosicionamento.Should().Be(frasePosicionamento);
        result.Value.SintesePerfil.Should().Be(sintesePerfil);
        result.Value.IdentidadeMarca.Should().Be(identidade);
        result.Value.PilaresConteudo.Should().HaveCount(4);
        result.Value.MatrizRestricoes.Should().Be(restricoes);
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.Empty;
        var anamneseId = Guid.NewGuid();

        // Act
        var result = PersonaDiagnosis.Create(
            tenantId,
            anamneseId,
            "Frase test",
            "Síntese test",
            new IdentidadeMarca("Tom", "Estilo", "Arquétipo A", "Arquétipo B"),
            new List<PilarConteudo> { new PilarConteudo("Geral", 100, "Desc", Array.Empty<string>()) },
            new MatrizRestricoes(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "Nenhum")
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.TenantIdInvalido");
    }

    [Fact]
    public void Create_WhenPilaresSumNot100Percent_ShouldReturnFailure()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();
        var pilaresInvalidos = new List<PilarConteudo>
        {
            new PilarConteudo("Pilar A", 40, "Desc A", Array.Empty<string>()),
            new PilarConteudo("Pilar B", 40, "Desc B", Array.Empty<string>())
        };

        // Act
        var result = PersonaDiagnosis.Create(
            tenantId,
            anamneseId,
            "Frase",
            "Síntese",
            new IdentidadeMarca("Tom", "Estilo", "Arq 1", "Arq 2"),
            pilaresInvalidos,
            new MatrizRestricoes(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), "Limites")
        );

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Personas.PercentualPilaresInvalido");
    }
}
