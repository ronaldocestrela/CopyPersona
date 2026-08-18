using Bunit;
using FluentAssertions;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Server.Components.Anamnese.Steps;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class AnamneseStepComponentsTests : BunitContext
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step1Component_ShouldRenderFieldsAndTriggerModelChanged()
    {
        Etapa1Dto? updatedModel = null;
        var initialModel = new Etapa1Dto("Dra. Mariana", "Dra. Mari", "Dentista", 5, "Pós em Ortodontia", "Prêmio Excelência", 40, MomentoAtualEnum.AgendaRazoavel);

        var cut = Render<Step1Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        cut.Find("h3").TextContent.Should().Contain("Etapa 1 — Quem é você");
        
        var nameInput = cut.Find("input[placeholder*='Mariana']");
        nameInput.Change("Dra. Mariana Silva");

        updatedModel.Should().NotBeNull();
        updatedModel!.NomeCompleto.Should().Be("Dra. Mariana Silva");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step8Component_ShouldSupportMultiSelectArquetipos()
    {
        Etapa8Dto? updatedModel = null;
        var initialModel = new Etapa8Dto(new List<ArquetipoComunicacaoEnum> { ArquetipoComunicacaoEnum.Autoridade }, "Amostra", "Identidade", "Cores vibrantes");

        var cut = Render<Step8Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        // Click second card (Amigo)
        var cards = cut.FindAll(".anamnese-option-card");
        cards[1].Click();

        updatedModel.Should().NotBeNull();
        updatedModel!.ArquetiposComunicacao.Should().Contain(ArquetipoComunicacaoEnum.Autoridade);
        updatedModel.ArquetiposComunicacao.Should().Contain(ArquetipoComunicacaoEnum.Amigo);
    }
}
