using FluentAssertions;
using PersonaScript.Modules.Scripts.Domain;
using Xunit;

namespace PersonaScript.Modules.Scripts.Tests.Domain;

public class VideoScriptTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenTenantIdIsEmpty()
    {
        // Act
        var result = VideoScript.Create(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Tema de Teste",
            "Educação",
            "Engajamento",
            "Gancho impactante",
            "Desenvolvimento do tema",
            "CTA ética",
            "Legenda sugerida",
            "Dicas de gravação",
            "Tom empático");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.TenantIdInvalido);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldReturnFailure_WhenTemaIsInvalid(string? temaInvalido)
    {
        // Act
        var result = VideoScript.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            temaInvalido!,
            "Educação",
            "Engajamento",
            "Gancho impactante",
            "Desenvolvimento do tema",
            "CTA ética",
            "Legenda sugerida",
            "Dicas de gravação",
            "Tom empático");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.TemaInvalido);
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenGanchoIsEmpty()
    {
        // Act
        var result = VideoScript.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Tema Válido",
            "Educação",
            "Engajamento",
            "",
            "Desenvolvimento do tema",
            "CTA ética",
            "Legenda sugerida",
            "Dicas de gravação",
            "Tom empático");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.Scripts.ConteudoObrigatorioInvalido);
    }

    [Fact]
    public void Create_ShouldReturnSuccessWithDraftStatus_WhenParametersAreValid()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamneseId = Guid.NewGuid();
        var diagnosisId = Guid.NewGuid();

        // Act
        var result = VideoScript.Create(
            tenantId,
            anamneseId,
            diagnosisId,
            "Como superar a ansiedade de falar em público",
            "Educação & Esclarecimento",
            "Autoridade e Conexão",
            "Você congela quando precisa falar na frente de várias pessoas?",
            "Neste vídeo explico a técnica dos 3 segundos...",
            "Comente 'EU' para receber nosso guia gratuito.",
            "Confira estas dicas práticas de oratória!",
            "Olhe diretamente para a câmera e fale pausadamente.",
            "Acolhedor e Seguro");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var script = result.Value;
        script.Should().NotBeNull();
        script.Id.Should().NotBeEmpty();
        script.TenantId.Should().Be(tenantId);
        script.AnamneseId.Should().Be(anamneseId);
        script.PersonaDiagnosisId.Should().Be(diagnosisId);
        script.Tema.Should().Be("Como superar a ansiedade de falar em público");
        script.PilarConteudo.Should().Be("Educação & Esclarecimento");
        script.Objetivo.Should().Be("Autoridade e Conexão");
        script.Gancho.Should().Be("Você congela quando precisa falar na frente de várias pessoas?");
        script.Retencao.Should().Be("Neste vídeo explico a técnica dos 3 segundos...");
        script.ChamadaParaAcao.Should().Be("Comente 'EU' para receber nosso guia gratuito.");
        script.LegendaSugerida.Should().Be("Confira estas dicas práticas de oratória!");
        script.DicasGravacao.Should().Be("Olhe diretamente para a câmera e fale pausadamente.");
        script.TomVozAplicado.Should().Be("Acolhedor e Seguro");
        script.Status.Should().Be(VideoScriptStatus.Draft);
    }

    [Fact]
    public void UpdateStatus_ShouldTransitionStatus_WhenTransitionIsValid()
    {
        // Arrange
        var script = CreateValidScript();

        // Act & Assert
        var approveResult = script.UpdateStatus(VideoScriptStatus.Approved);
        approveResult.IsSuccess.Should().BeTrue();
        script.Status.Should().Be(VideoScriptStatus.Approved);

        var recordResult = script.UpdateStatus(VideoScriptStatus.Recorded);
        recordResult.IsSuccess.Should().BeTrue();
        script.Status.Should().Be(VideoScriptStatus.Recorded);

        var publishResult = script.UpdateStatus(VideoScriptStatus.Published);
        publishResult.IsSuccess.Should().BeTrue();
        script.Status.Should().Be(VideoScriptStatus.Published);
    }

    [Fact]
    public void UpdateStatus_ShouldReturnFailure_WhenTransitionIsInvalid()
    {
        // Arrange
        var script = CreateValidScript();
        script.UpdateStatus(VideoScriptStatus.Approved);
        script.UpdateStatus(VideoScriptStatus.Recorded);
        script.UpdateStatus(VideoScriptStatus.Published);

        // Act - tentar voltar de Published para Draft
        var invalidResult = script.UpdateStatus(VideoScriptStatus.Draft);

        // Assert
        invalidResult.IsFailure.Should().BeTrue();
        invalidResult.Error.Should().Be(DomainErrors.Scripts.StatusTransicaoInvalida);
        script.Status.Should().Be(VideoScriptStatus.Published);
    }

    [Fact]
    public void UpdateContent_ShouldUpdateFieldsAndTimestamp_WhenValid()
    {
        // Arrange
        var script = CreateValidScript();
        var novoGancho = "Novo gancho ainda mais magnético!";
        var novaRetencao = "Nova retenção aprofundada.";
        var novaCta = "Clique no link da bio.";

        // Act
        var result = script.UpdateContent(
            "Novo Tema",
            "Novo Pilar",
            "Novo Objetivo",
            novoGancho,
            novaRetencao,
            novaCta,
            "Nova legenda",
            "Novas dicas",
            "Novo tom");

        // Assert
        result.IsSuccess.Should().BeTrue();
        script.Tema.Should().Be("Novo Tema");
        script.Gancho.Should().Be(novoGancho);
        script.Retencao.Should().Be(novaRetencao);
        script.ChamadaParaAcao.Should().Be(novaCta);
        script.AtualizadoEm.Should().NotBeNull();
    }

    private static VideoScript CreateValidScript()
    {
        return VideoScript.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Tema Inicial",
            "Educação",
            "Conversão",
            "Gancho inicial",
            "Retenção inicial",
            "CTA inicial",
            "Legenda",
            "Dicas",
            "Tom").Value;
    }
}
