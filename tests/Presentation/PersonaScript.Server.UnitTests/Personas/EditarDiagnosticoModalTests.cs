using Bunit;
using FluentAssertions;
using PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Server.Components.Pages.Posicionamento;
using Xunit;

namespace PersonaScript.Server.UnitTests.Personas;

public class EditarDiagnosticoModalTests : BunitContext
{
    private static PersonaDiagnosisDto CreateSampleDiagnosis()
    {
        return new PersonaDiagnosisDto(
            Id: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            AnamneseId: Guid.NewGuid(),
            FrasePosicionamento: "Odontologia humanizada para devolver a autoestima.",
            SintesePerfil: "Especialista em reabilitação oral e estética dental.",
            IdentidadeMarca: new IdentidadeMarcaDto(
                TomDeVoz: "Acolhedor e técnico",
                EstiloVisualSugerido: "Clean, tons neutros",
                ArquetipoPrincipal: "O Cuidador",
                ArquetipoSecundario: "O Sábio"
            ),
            PilaresConteudo: new List<PilarConteudoDto>
            {
                new("Autoridade Técnica", 40, "Casos clínicos complexos", new List<string> { "Antes e Depois", "Tecnologia 3D" }),
                new("Conexão e Humanização", 30, "Histórias de pacientes", new List<string> { "Bastidores", "Depoimentos" }),
                new("Educação do Paciente", 30, "Mitos e verdades", new List<string> { "Higiene oral", "Prevenção" })
            },
            MatrizRestricoes: new MatrizRestricoesDto(
                TemasProibidos: new List<string> { "Preços de procedimentos", "Comparações depreciativas" },
                PalavrasEvitar: new List<string> { "Barato", "Garantia de resultado" },
                DiretrizesInegociaveis: new List<string> { "Respeito ao código de ética do CRO" },
                LimitesExposicao: "Não expor fotos não autorizadas de pacientes"
            ),
            GeradoEm: DateTimeOffset.UtcNow,
            AtualizadoEm: null
        );
    }

    [Fact]
    public void Modal_WhenClosed_ShouldNotRenderContent()
    {
        // Act
        var cut = Render<EditarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.Diagnosis, CreateSampleDiagnosis()));

        // Assert
        cut.FindAll(".modal-backdrop").Should().BeEmpty();
    }

    [Fact]
    public void Modal_WhenOpen_ShouldRenderDiagnosisFields()
    {
        // Arrange
        var diagnosis = CreateSampleDiagnosis();

        // Act
        var cut = Render<EditarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Diagnosis, diagnosis));

        // Assert
        cut.Find(".modal-header h2").TextContent.Should().Contain("Editar Diagnóstico de Posicionamento");
        cut.Find("textarea").GetAttribute("value").Should().Contain(diagnosis.FrasePosicionamento);
        cut.Find(".modal-body").TextContent.Should().Contain("Atual: 100%");
    }

    [Fact]
    public void Modal_WhenPilaresSumIsNot100_ShouldDisplayErrorMessageOnSave()
    {
        // Arrange
        var diagnosis = CreateSampleDiagnosis();
        UpdatePersonaDiagnosisCommand? savedCommand = null;

        var cut = Render<EditarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Diagnosis, diagnosis)
            .Add(p => p.OnSaved, cmd => savedCommand = cmd));

        // Act - alterar percentual do primeiro pilar para 80 (total = 80 + 30 + 30 = 140%)
        var percentInput = cut.Find("input[type='number']");
        percentInput.Change(80);

        var saveButton = cut.Find("button.btn-primary");
        saveButton.Click();

        // Assert
        cut.Find(".alert-error").TextContent.Should().Contain("A soma dos percentuais dos pilares de conteúdo deve ser exatamente 100%.");
        savedCommand.Should().BeNull();
    }

    [Fact]
    public void Modal_WhenValid_ShouldEmitOnSavedWithUpdatedCommand()
    {
        // Arrange
        var diagnosis = CreateSampleDiagnosis();
        UpdatePersonaDiagnosisCommand? savedCommand = null;

        var cut = Render<EditarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Diagnosis, diagnosis)
            .Add(p => p.OnSaved, cmd => savedCommand = cmd));

        // Act - Salvar com valores válidos (100%)
        var saveButton = cut.Find("button.btn-primary");
        saveButton.Click();

        // Assert
        savedCommand.Should().NotBeNull();
        savedCommand!.FrasePosicionamento.Should().Be(diagnosis.FrasePosicionamento);
        savedCommand.PilaresConteudo.Sum(p => p.Percentual).Should().Be(100);
        savedCommand.IdentidadeMarca.TomDeVoz.Should().Be("Acolhedor e técnico");
    }

    [Fact]
    public void Modal_ClickingClose_ShouldTriggerOnClosedCallback()
    {
        // Arrange
        var closed = false;
        var cut = Render<EditarDiagnosticoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Diagnosis, CreateSampleDiagnosis())
            .Add(p => p.OnClosed, () => closed = true));

        // Act
        var closeButton = cut.Find("button.modal-close-btn");
        closeButton.Click();

        // Assert
        closed.Should().BeTrue();
    }
}
