using Bunit;
using FluentAssertions;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Server.Components.Pages.Roteiros;
using Xunit;

namespace PersonaScript.Server.UnitTests.Roteiros;

public class ExportModalTests : BunitContext
{
    private static VideoScriptDto CreateSampleScript()
    {
        return new VideoScriptDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AnamneseId: Guid.NewGuid(),
            PersonaDiagnosisId: Guid.NewGuid(),
            Tema: "Sensibilidade nos Dentes ao Tomar Café",
            PilarConteudo: "Educação do Paciente",
            Objetivo: "Atrair pacientes com dor e conscientizar sobre retração gengival",
            Gancho: "Seu dente dá aquele choque quando você toma um café quente?",
            Retencao: "Isso geralmente acontece quando a gengiva retrai e expõe os túbulos dentinários.",
            ChamadaParaAcao: "Comente 'CONFORTO' para agendar uma avaliação e eliminar essa dor.",
            LegendaSugerida: "Não ignore a sensibilidade nos dentes! Saiba as causas e tratamentos.",
            DicasGravacao: "Grave na clínica, segurando uma xícara de café para reforçar o contexto visual.",
            TomVozAplicado: "Didático e acolhedor",
            Status: VideoScriptStatus.Draft,
            FeedbackRating: ScriptFeedbackRating.None,
            FeedbackNotes: null,
            FeedbackAt: null,
            GeradoEm: DateTimeOffset.UtcNow,
            AtualizadoEm: null
        );
    }

    [Fact]
    public void Modal_WhenNotVisible_ShouldNotRenderContent()
    {
        // Act
        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Script, CreateSampleScript()));

        // Assert
        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenVisible_ShouldRenderScriptThemeAndExportOptions()
    {
        // Arrange
        var script = CreateSampleScript();

        // Act
        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, script));

        // Assert
        cut.Find(".modal-title").TextContent.Should().Contain("Exportar Roteiro");
        cut.Find(".modal-body").TextContent.Should().Contain(script.Tema);
        cut.FindAll("button:contains('Copiar Texto Completo')").Should().NotBeEmpty();
        cut.FindAll("button:contains('Baixar Markdown (.md)')").Should().NotBeEmpty();
        cut.FindAll("button:contains('Imprimir / Salvar em PDF')").Should().NotBeEmpty();
    }

    [Fact]
    public void Modal_ClickingCopyFullScript_ShouldInvokeClipboardAndShowToast()
    {
        // Arrange
        var script = CreateSampleScript();
        JSInterop.Setup<bool>("exportHelper.copyToClipboard", _ => true).SetResult(true);

        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, script));

        // Act
        var copyBtn = cut.Find("button:contains('Copiar Texto Completo')");
        copyBtn.Click();

        // Assert
        JSInterop.VerifyInvoke("exportHelper.copyToClipboard", 1);
        cut.Find(".alert-success").TextContent.Should().Contain("Roteiro copiado para a área de transferência com sucesso!");
    }

    [Fact]
    public void Modal_ClickingDownloadMarkdown_ShouldInvokeFileDownloadAndShowToast()
    {
        // Arrange
        var script = CreateSampleScript();
        JSInterop.SetupVoid("exportHelper.downloadFile", _ => true).SetVoidResult();

        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, script));

        // Act
        var downloadBtn = cut.Find("button:contains('Baixar Markdown (.md)')");
        downloadBtn.Click();

        // Assert
        JSInterop.VerifyInvoke("exportHelper.downloadFile", 1);
        cut.Find(".alert-success").TextContent.Should().Contain("Arquivo Markdown (.md) baixado com sucesso!");
    }

    [Fact]
    public void Modal_ClickingPrintPdf_ShouldInvokePrintPage()
    {
        // Arrange
        var script = CreateSampleScript();
        JSInterop.SetupVoid("exportHelper.printPage").SetVoidResult();

        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, script));

        // Act
        var printBtn = cut.Find("button:contains('Imprimir / Salvar em PDF')");
        printBtn.Click();

        // Assert
        JSInterop.VerifyInvoke("exportHelper.printPage", 1);
    }

    [Fact]
    public void Modal_ClickingClose_ShouldTriggerOnCloseCallback()
    {
        // Arrange
        var closed = false;
        var cut = Render<ExportModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Script, CreateSampleScript())
            .Add(p => p.OnClose, () => closed = true));

        // Act
        cut.Find("button.btn-close").Click();

        // Assert
        closed.Should().BeTrue();
    }
}
