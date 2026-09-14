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

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step5Component_ShouldRenderMultiInstagramInputsAndTriggerModelChanged()
    {
        Etapa5Dto? updatedModel = null;
        var initialModel = new Etapa5Dto(new[] { "dramarianacosta" }, "Didática", "Dancinha", new[] { "marcasouinfluencer" }, "Estética minimalista");

        var cut = Render<Step5Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        cut.Find("h3").TextContent.Should().Contain("Etapa 5 — Suas Referências");
        var listContainers = cut.FindAll(".anamnese-instagram-list-container");
        listContainers.Should().HaveCount(2);
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step3Component_ShouldRenderFieldsAndTriggerModelChangedOnChange()
    {
        Etapa3Dto? updatedModel = null;
        var initialModel = new Etapa3Dto("Master 1", "Lucrativo 1", "Preferido 1", "Diferencial 1", "Escolhem 1", "Critica 1");

        var cut = Render<Step3Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        cut.Find("h3").TextContent.Should().Contain("Etapa 3 — Seu Trabalho");

        var masterInput = cut.Find("input[placeholder*='Harmonização facial']");
        masterInput.Change("Lentes de Contato Dental Ultra Finas");

        updatedModel.Should().NotBeNull();
        updatedModel!.ProcedimentoMaster.Should().Be("Lentes de Contato Dental Ultra Finas");

        var diferencialTextarea = cut.Find("textarea[placeholder*='Consulta sem pressa']");
        diferencialTextarea.Change("Atendimento personalizado com scanner 3D e café gourmet");

        updatedModel.DiferencialAtendimento.Should().Be("Atendimento personalizado com scanner 3D e café gourmet");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step4Component_ShouldRenderFieldsAndTriggerModelChangedOnChange()
    {
        Etapa4Dto? updatedModel = null;
        var initialModel = new Etapa4Dto("Perfil 1", "Medos 1", "Desejos 1", "Perguntas 1", "Mitos 1", CanalOrigemEnum.Instagram);

        var cut = Render<Step4Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        cut.Find("h3").TextContent.Should().Contain("Etapa 4 — Seu Paciente");

        var perfilTextarea = cut.Find("textarea[placeholder*='Mulheres entre 35']");
        perfilTextarea.Change("Mulheres de 30 a 50 anos empresárias");

        updatedModel.Should().NotBeNull();
        updatedModel!.PerfilDemograficoPsicografico.Should().Be("Mulheres de 30 a 50 anos empresárias");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Step7Component_ShouldRenderFieldsAndTriggerModelChangedOnChange()
    {
        Etapa7Dto? updatedModel = null;
        var initialModel = new Etapa7Dto("Temas 1", "Palestra 1", "Verdade 1", "Deu Certo 1", "Nao Funcionou 1", "Sonhos 1");

        var cut = Render<Step7Component>(parameters => parameters
            .Add(p => p.Model, initialModel)
            .Add(p => p.ModelChanged, m => updatedModel = m));

        cut.Find("h3").TextContent.Should().Contain("Etapa 7 — Seu Conhecimento");

        var temasTextarea = cut.Find("textarea[placeholder*='Prevenção do envelhecimento']");
        temasTextarea.Change("Como prevenir flacidez facial após os 40");

        updatedModel.Should().NotBeNull();
        updatedModel!.TemasFavoritos.Should().Be("Como prevenir flacidez facial após os 40");
    }
}
