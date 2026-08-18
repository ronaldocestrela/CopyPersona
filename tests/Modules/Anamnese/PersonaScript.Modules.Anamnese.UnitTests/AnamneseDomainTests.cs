using FluentAssertions;
using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;
using Xunit;

namespace PersonaScript.Modules.Anamnese.UnitTests;

public class AnamneseDomainTests
{
    [Fact]
    public void Create_ComTenantIdValido_DeveInicializarComSucessoEStatusRascunho()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var result = Domain.Anamnese.Create(tenantId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var anamnese = result.Value;
        anamnese.TenantId.Should().Be(tenantId);
        anamnese.Status.Should().Be(AnamneseStatus.Rascunho);
        anamnese.EtapaAtual.Should().Be(1);
        anamnese.PercentualConclusao.Should().Be(0);
        anamnese.CriadoEm.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        anamnese.ConcluidoEm.Should().BeNull();
    }

    [Fact]
    public void Create_ComTenantIdVazio_DeveRetornarFalha()
    {
        // Act
        var result = Domain.Anamnese.Create(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Anamnese.TenantIdInvalido");
    }

    [Fact]
    public void UpdateEtapa1_ComDadosValidos_DeveAtualizarEtapaEPercentualConclusao()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamnese = Domain.Anamnese.Create(tenantId).Value;
        var etapa1 = new Etapa1QuemEVoce(
            NomeCompleto: "Dra. Mariana Costa",
            ComoGostaSerChamado: "Dra. Mari",
            ProfissaoEspecialidade: "Dentista especialista em lentes de contato dental",
            TempoAtuacaoAnos: 8,
            FormacoesEspecializacoes: "Pós-graduação em Odontologia Estética",
            PremiosTitulos: "Referência Regional em Estética",
            PacientesMes: 40,
            MomentoAtual: MomentoAtualEnum.AgendaRazoavel
        );

        // Act
        var result = anamnese.UpdateEtapa1(etapa1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        anamnese.Etapa1.Should().Be(etapa1);
        anamnese.PercentualConclusao.Should().Be(10);
        anamnese.EtapaAtual.Should().Be(2);
        anamnese.AtualizadoEm.Should().NotBeNull();
    }

    [Fact]
    public void UpdateEtapas_PreenchendoTodasAsEtapas_DeveCalcular100PercentoEConcluirComSucesso()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamnese = Domain.Anamnese.Create(tenantId).Value;

        // Act
        anamnese.UpdateEtapa1(new Etapa1QuemEVoce("Dra. Mari", "Mari", "Dentista", 5, "Pós", "Título", 30, MomentoAtualEnum.IniciandoAgenda));
        anamnese.UpdateEtapa2(new Etapa2SuaHistoria("Motivação", "Caso Marcante", "Fase Difícil", "Motor Pessoal"));
        anamnese.UpdateEtapa3(new Etapa3SeuTrabalho("Lentes", "Implantes", "Lentes", "Atendimento VIP", "Confiança", "Promessas falsas"));
        anamnese.UpdateEtapa4(new Etapa4SeuPaciente("Mulher 30-45", "Dor, Artificial", "Rejuvenescer", "Dói? Quanto tempo?", "Botox trava rosto", CanalOrigemEnum.Instagram));
        anamnese.UpdateEtapa5(new Etapa5SuasReferencias("https://instagram.com/prof1, https://instagram.com/prof2", "Didática", "Dancinha", "https://instagram.com/marca1", "Estética minimalista"));
        anamnese.UpdateEtapa6(new Etapa6LimitesExposicao("Política, Religião", "Família, Casa", "Viagens", "Bastidores", NivelConfortoCameraEnum.SuperAVontade, "Regras CRO"));
        anamnese.UpdateEtapa7(new Etapa7SeuConhecimento("Harmonização, Lentes, Botox, Preenchimento, Clareamento", "Mitos da Harmonização", "Procedimentos desnecessários vendidos", "Post sobre Mitos", "Post teórico longo", "Vídeo de bastidores"));
        anamnese.UpdateEtapa8(new Etapa8SeuJeito(new[] { ArquetipoComunicacaoEnum.Professor, ArquetipoComunicacaoEnum.Amigo }, "Explico de forma direta e acolhedora sem jargões", "Gosto da minha identidade atual", "Odeio cores chamativas"));
        anamnese.UpdateEtapa9(new Etapa9RotinaCapacidade("Acordo 6h, atendo 8h às 18h", "3 horas por semana", "Secretária", "1. Fotos, 2. Textos, 3. Vídeos", "5 stories por dia, 2 reels na semana"));
        anamnese.UpdateEtapa10(new Etapa10Objetivos("10 novos pacientes/mês", "Agenda lotada", "Contratei agência que usava posts prontos", ResultadoPrioritarioEnum.PacientesMelhoresTicketAlto));

        // Assert progress
        anamnese.PercentualConclusao.Should().Be(100);

        // Act Concluir
        var concluirResult = anamnese.Concluir();

        // Assert conclusion
        concluirResult.IsSuccess.Should().BeTrue();
        anamnese.Status.Should().Be(AnamneseStatus.Concluido);
        anamnese.ConcluidoEm.Should().NotBeNull();
    }

    [Fact]
    public void Concluir_ComEtapasIncompletas_DeveRetornarFalha()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamnese = Domain.Anamnese.Create(tenantId).Value;
        anamnese.UpdateEtapa1(new Etapa1QuemEVoce("Dra. Mari", "Mari", "Dentista", 5, "Pós", "Título", 30, MomentoAtualEnum.IniciandoAgenda));

        // Act
        var result = anamnese.Concluir();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Anamnese.EtapasIncompletas");
        anamnese.Status.Should().Be(AnamneseStatus.Rascunho);
    }

    [Fact]
    public void UpdateEtapa_QuandoJaConcluido_DeveRetornarFalha()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var anamnese = Domain.Anamnese.Create(tenantId).Value;
        anamnese.UpdateEtapa1(new Etapa1QuemEVoce("Dra. Mari", "Mari", "Dentista", 5, "Pós", "Título", 30, MomentoAtualEnum.IniciandoAgenda));
        anamnese.UpdateEtapa2(new Etapa2SuaHistoria("Motivação", "Caso Marcante", "Fase Difícil", "Motor Pessoal"));
        anamnese.UpdateEtapa3(new Etapa3SeuTrabalho("Lentes", "Implantes", "Lentes", "Atendimento VIP", "Confiança", "Promessas falsas"));
        anamnese.UpdateEtapa4(new Etapa4SeuPaciente("Mulher 30-45", "Dor, Artificial", "Rejuvenescer", "Dói? Quanto tempo?", "Botox trava rosto", CanalOrigemEnum.Instagram));
        anamnese.UpdateEtapa5(new Etapa5SuasReferencias("https://instagram.com/prof1", "Didática", "Dancinha", "https://instagram.com/marca1", "Estética minimalista"));
        anamnese.UpdateEtapa6(new Etapa6LimitesExposicao("Política", "Família", "Viagens", "Bastidores", NivelConfortoCameraEnum.SuperAVontade, "Regras CRO"));
        anamnese.UpdateEtapa7(new Etapa7SeuConhecimento("Harmonização", "Mitos", "Procedimentos desnecessários", "Post sobre Mitos", "Post longo", "Vídeo bastidores"));
        anamnese.UpdateEtapa8(new Etapa8SeuJeito(new[] { ArquetipoComunicacaoEnum.Professor }, "Acolhedora", "Gosto da atual", "Odeio chamativas"));
        anamnese.UpdateEtapa9(new Etapa9RotinaCapacidade("Dia típico", "3h/semana", "Secretária", "Fotos", "5 stories/dia"));
        anamnese.UpdateEtapa10(new Etapa10Objetivos("10 pacientes", "Agenda lotada", "Agência anterior", ResultadoPrioritarioEnum.PacientesMelhoresTicketAlto));
        anamnese.Concluir();

        // Act
        var result = anamnese.UpdateEtapa1(new Etapa1QuemEVoce("Nova Dra.", "Mari", "Dentista", 6, "Pós", "Título", 35, MomentoAtualEnum.AgendaRazoavel));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Anamnese.JaConcluida");
    }
}
