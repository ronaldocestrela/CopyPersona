using FluentAssertions;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.AnalyzeStepClarification;
using PersonaScript.Modules.Anamnese.Application.Services;
using PersonaScript.Modules.Anamnese.Domain;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests.Queries;

public class ClarificationAnalyzerTests
{
    private readonly HeuristicClarificationAnalyzer _analyzer;
    private readonly AnamneseClarificationService _service;

    public ClarificationAnalyzerTests()
    {
        _analyzer = new HeuristicClarificationAnalyzer();
        _service = new AnamneseClarificationService(_analyzer);
    }

    [Fact]
    public async Task Step3_ComRespostaVagaEMuitoCurta_DeveDetectarVagueza()
    {
        // Arrange
        var etapa3Dto = new Etapa3Dto(
            ProcedimentoMaster: "Ortodontia",
            ProcedimentoLucrativo: "Aparelhos invisíveis",
            ProcedimentoPreferido: "Alinhadores",
            DiferencialAtendimento: "Atendimento presencial",
            PorQueEscolhemVoce: "Sou dedicado e atendo com amor", // Vago & clichê
            CriticaAosPares: "Falta de pontualidade"
        );

        var query = new AnalyzeStepClarificationQuery(3, etapa3Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeTrue();
        result.Value.Items.Should().NotBeEmpty();
        
        var item = result.Value.Items.First();
        item.QuestionId.Should().Be("3.5");
        item.FieldName.Should().Be("PorQueEscolhemVoce");
        item.ReasonVague.Should().Contain("genérica");
        item.SuggestionPrompt.Should().NotBeNullOrEmpty();
        item.ExampleAnswer.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Step3_ComRespostaDetada_NaoDeveDetectarVagueza()
    {
        // Arrange
        var etapa3Dto = new Etapa3Dto(
            ProcedimentoMaster: "Ortodontia",
            ProcedimentoLucrativo: "Aparelhos invisíveis",
            ProcedimentoPreferido: "Alinhadores",
            DiferencialAtendimento: "Atendimento presencial",
            PorQueEscolhemVoce: "Desenvolvi um protocolo digital de alinhadores com simulação 3D na 1ª consulta e acompanhamento quinzenal exclusivo via aplicativo.",
            CriticaAosPares: "Falta de pontualidade"
        );

        var query = new AnalyzeStepClarificationQuery(3, etapa3Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Step4_ComDorDoPacienteVaga_DeveDetectarVagueza()
    {
        // Arrange
        var etapa4Dto = new Etapa4Dto(
            PerfilDemograficoPsicografico: "Mulheres de 30 a 50 anos",
            MaioresMedos: "Quer melhorar de vida", // Vagueza
            MaioresDesejos: "Autoestima alta",
            PerguntasFrequentes: "Quanto custa?",
            MitosInformacoesErradas: "Dói para aplicar?",
            CanalOrigem: CanalOrigemEnum.Instagram
        );

        var query = new AnalyzeStepClarificationQuery(4, etapa4Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeTrue();
        result.Value.Items.Should().Contain(x => x.QuestionId == "4.2");
    }

    [Fact]
    public async Task Step7_ComMetodologiaVaga_DeveDetectarVagueza()
    {
        // Arrange
        var etapa7Dto = new Etapa7Dto(
            TemasFavoritos: "Estética",
            TemaPalestra: "Harmonização Natural",
            VerdadeCorajosa: "Aplico técnicas modernas", // Vagueza
            PostsDeuCerto: "Casos antes e depois",
            PostsNaoFuncionaram: "Posts informativos estáticos",
            ConteudoDosSonhos: "Série em vídeo no Reels"
        );

        var query = new AnalyzeStepClarificationQuery(7, etapa7Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeTrue();
        result.Value.Items.Should().Contain(x => x.QuestionId == "7.3");
    }

    [Fact]
    public async Task Step8_ComAmostraDeEscritaInsuficiente_DeveDetectarVagueza()
    {
        // Arrange
        var etapa8Dto = new Etapa8Dto(
            ArquetiposComunicacao: new List<ArquetipoComunicacaoEnum> { ArquetipoComunicacaoEnum.Autoridade },
            AmostraEscritaExplicativa: "Falo normal", // Muito curto (< 40 caracteres)
            IdentidadeVisualStatus: "Definida",
            EsteticaOdiada: "Cores neon e exagero"
        );

        var query = new AnalyzeStepClarificationQuery(8, etapa8Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeTrue();
        result.Value.Items.Should().Contain(x => x.QuestionId == "8.2");
    }

    [Fact]
    public async Task Step8_ComAmostraDeEscritaCompleta_NaoDeveDetectarVagueza()
    {
        // Arrange
        var etapa8Dto = new Etapa8Dto(
            ArquetiposComunicacao: new List<ArquetipoComunicacaoEnum> { ArquetipoComunicacaoEnum.Professor },
            AmostraEscritaExplicativa: "Olá! Sempre digo aos meus alunos que a constância vence o talento quando o talento não tem disciplina. Se você quer transformar seus resultados, comece hoje mesmo pelo básico bem feito.",
            IdentidadeVisualStatus: "Definida",
            EsteticaOdiada: "Cores neon e exagero"
        );

        var query = new AnalyzeStepClarificationQuery(8, etapa8Dto);
        var handler = new AnalyzeStepClarificationQueryHandler(_service);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsVague.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
    }
}
