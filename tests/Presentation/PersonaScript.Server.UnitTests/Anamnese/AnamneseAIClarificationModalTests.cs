using Bunit;
using FluentAssertions;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Server.Components.Anamnese;
using Xunit;

namespace PersonaScript.Server.UnitTests.Anamnese;

public class AnamneseAIClarificationModalTests : BunitContext
{
    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Modal_WhenNotVisible_ShouldRenderNothing()
    {
        var cut = Render<AnamneseAIClarificationModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Item, null)
        );

        cut.FindAll(".anamnese-ai-modal-card").Should().BeEmpty();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Modal_WhenVisibleWithItem_ShouldRenderDetails()
    {
        var item = new ClarificationItemDto(
            QuestionId: "3.5",
            FieldName: "PorQueEscolhemVoce",
            CurrentAnswer: "Sou dedicado",
            ReasonVague: "Resposta genérica",
            SuggestionTitle: "Aprofunde seu diferencial",
            SuggestionPrompt: "Como é sua 1ª consulta?",
            ExampleAnswer: "Exemplo: protocolo digital"
        );

        var cut = Render<AnamneseAIClarificationModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Item, item)
        );

        cut.Find(".anamnese-ai-title").TextContent.Should().Be("Aprofunde seu diferencial");
        cut.Find(".anamnese-ai-reason").TextContent.Should().Contain("Resposta genérica");
        cut.Find(".anamnese-ai-prompt-box").TextContent.Should().Contain("Como é sua 1ª consulta?");
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Modal_ClickingAdjust_ShouldTriggerOnAdjustCallback()
    {
        var item = new ClarificationItemDto("3.5", "PorQueEscolhemVoce", "Sou dedicado", "Razão", "Título", "Prompt", "Exemplo");
        var adjustTriggered = false;

        var cut = Render<AnamneseAIClarificationModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Item, item)
            .Add(p => p.OnAdjust, () => adjustTriggered = true)
        );

        cut.Find("button:contains('Ajustar minha resposta')").Click();

        adjustTriggered.Should().BeTrue();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "bUnit TestContext manages component lifecycle")]
    public void Modal_ClickingDismiss_ShouldTriggerOnDismissCallback()
    {
        var item = new ClarificationItemDto("3.5", "PorQueEscolhemVoce", "Sou dedicado", "Razão", "Título", "Prompt", "Exemplo");
        var dismissTriggered = false;

        var cut = Render<AnamneseAIClarificationModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Item, item)
            .Add(p => p.OnDismiss, () => dismissTriggered = true)
        );

        cut.Find("button:contains('Manter assim e continuar')").Click();

        dismissTriggered.Should().BeTrue();
    }
}
