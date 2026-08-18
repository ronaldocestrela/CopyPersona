using PersonaScript.Modules.Anamnese.Domain;
using PersonaScript.Modules.Anamnese.Domain.ValueObjects;

namespace PersonaScript.Modules.Anamnese.Application.DTOs;

public record Etapa1Dto(
    string NomeCompleto,
    string ComoGostaSerChamado,
    string ProfissaoEspecialidade,
    int TempoAtuacaoAnos,
    string FormacoesEspecializacoes,
    string PremiosTitulos,
    int PacientesMes,
    MomentoAtualEnum MomentoAtual
)
{
    public Etapa1QuemEVoce ToValueObject() => new(
        NomeCompleto, ComoGostaSerChamado, ProfissaoEspecialidade, TempoAtuacaoAnos,
        FormacoesEspecializacoes, PremiosTitulos, PacientesMes, MomentoAtual);

    public static Etapa1Dto FromValueObject(Etapa1QuemEVoce vo) => new(
        vo.NomeCompleto, vo.ComoGostaSerChamado, vo.ProfissaoEspecialidade, vo.TempoAtuacaoAnos,
        vo.FormacoesEspecializacoes, vo.PremiosTitulos, vo.PacientesMes, vo.MomentoAtual);
}

public record Etapa2Dto(
    string MotivacaoEscolha,
    string CasoMarcante,
    string FaseMaisDificil,
    string MotorPessoal
)
{
    public Etapa2SuaHistoria ToValueObject() => new(MotivacaoEscolha, CasoMarcante, FaseMaisDificil, MotorPessoal);
    public static Etapa2Dto FromValueObject(Etapa2SuaHistoria vo) => new(vo.MotivacaoEscolha, vo.CasoMarcante, vo.FaseMaisDificil, vo.MotorPessoal);
}

public record Etapa3Dto(
    string ProcedimentoMaster,
    string ProcedimentoLucrativo,
    string ProcedimentoPreferido,
    string DiferencialAtendimento,
    string PorQueEscolhemVoce,
    string CriticaAosPares
)
{
    public Etapa3SeuTrabalho ToValueObject() => new(ProcedimentoMaster, ProcedimentoLucrativo, ProcedimentoPreferido, DiferencialAtendimento, PorQueEscolhemVoce, CriticaAosPares);
    public static Etapa3Dto FromValueObject(Etapa3SeuTrabalho vo) => new(vo.ProcedimentoMaster, vo.ProcedimentoLucrativo, vo.ProcedimentoPreferido, vo.DiferencialAtendimento, vo.PorQueEscolhemVoce, vo.CriticaAosPares);
}

public record Etapa4Dto(
    string PerfilDemograficoPsicografico,
    string MaioresMedos,
    string MaioresDesejos,
    string PerguntasFrequentes,
    string MitosInformacoesErradas,
    CanalOrigemEnum CanalOrigem
)
{
    public Etapa4SeuPaciente ToValueObject() => new(PerfilDemograficoPsicografico, MaioresMedos, MaioresDesejos, PerguntasFrequentes, MitosInformacoesErradas, CanalOrigem);
    public static Etapa4Dto FromValueObject(Etapa4SeuPaciente vo) => new(vo.PerfilDemograficoPsicografico, vo.MaioresMedos, vo.MaioresDesejos, vo.PerguntasFrequentes, vo.MitosInformacoesErradas, vo.CanalOrigem);
}

public record Etapa5Dto(
    string PerfisArea,
    string OQueAdmiraArea,
    string OQueNaoFariaArea,
    string PerfisForaArea,
    string OQueAtraiForaArea
)
{
    public Etapa5SuasReferencias ToValueObject() => new(PerfisArea, OQueAdmiraArea, OQueNaoFariaArea, PerfisForaArea, OQueAtraiForaArea);
    public static Etapa5Dto FromValueObject(Etapa5SuasReferencias vo) => new(vo.PerfisArea, vo.OQueAdmiraArea, vo.OQueNaoFariaArea, vo.PerfisForaArea, vo.OQueAtraiForaArea);
}

public record Etapa6Dto(
    string AssuntosProibidos,
    string VidaPessoalAceita,
    string EstiloVidaAceito,
    string TrabalhoAceito,
    NivelConfortoCameraEnum NivelConfortoCamera,
    string RegrasConselhoRegional
)
{
    public Etapa6LimitesExposicao ToValueObject() => new(AssuntosProibidos, VidaPessoalAceita, EstiloVidaAceito, TrabalhoAceito, NivelConfortoCamera, RegrasConselhoRegional);
    public static Etapa6Dto FromValueObject(Etapa6LimitesExposicao vo) => new(vo.AssuntosProibidos, vo.VidaPessoalAceita, vo.EstiloVidaAceito, vo.TrabalhoAceito, vo.NivelConfortoCamera, vo.RegrasConselhoRegional);
}

public record Etapa7Dto(
    string TemasFavoritos,
    string TemaPalestra,
    string VerdadeCorajosa,
    string PostsDeuCerto,
    string PostsNaoFuncionaram,
    string ConteudoDosSonhos
)
{
    public Etapa7SeuConhecimento ToValueObject() => new(TemasFavoritos, TemaPalestra, VerdadeCorajosa, PostsDeuCerto, PostsNaoFuncionaram, ConteudoDosSonhos);
    public static Etapa7Dto FromValueObject(Etapa7SeuConhecimento vo) => new(vo.TemasFavoritos, vo.TemaPalestra, vo.VerdadeCorajosa, vo.PostsDeuCerto, vo.PostsNaoFuncionaram, vo.ConteudoDosSonhos);
}

public record Etapa8Dto(
    IReadOnlyCollection<ArquetipoComunicacaoEnum> ArquetiposComunicacao,
    string AmostraEscritaExplicativa,
    string IdentidadeVisualStatus,
    string EsteticaOdiada
)
{
    public Etapa8SeuJeito ToValueObject() => new(ArquetiposComunicacao, AmostraEscritaExplicativa, IdentidadeVisualStatus, EsteticaOdiada);
    public static Etapa8Dto FromValueObject(Etapa8SeuJeito vo) => new(vo.ArquetiposComunicacao, vo.AmostraEscritaExplicativa, vo.IdentidadeVisualStatus, vo.EsteticaOdiada);
}

public record Etapa9Dto(
    string DiaTipicoRotina,
    string HorasSemanaConteudo,
    string ApoioDisponivel,
    string RankingFacilidadeFormatos,
    string HistoricoPostagensUltimaSemana
)
{
    public Etapa9RotinaCapacidade ToValueObject() => new(DiaTipicoRotina, HorasSemanaConteudo, ApoioDisponivel, RankingFacilidadeFormatos, HistoricoPostagensUltimaSemana);
    public static Etapa9Dto FromValueObject(Etapa9RotinaCapacidade vo) => new(vo.DiaTipicoRotina, vo.HorasSemanaConteudo, vo.ApoioDisponivel, vo.RankingFacilidadeFormatos, vo.HistoricoPostagensUltimaSemana);
}

public record Etapa10Dto(
    string Meta3Meses,
    string Meta1Ano,
    string ExperienciaPassadaMarketing,
    ResultadoPrioritarioEnum ResultadoPrioritario
)
{
    public Etapa10Objetivos ToValueObject() => new(Meta3Meses, Meta1Ano, ExperienciaPassadaMarketing, ResultadoPrioritario);
    public static Etapa10Dto FromValueObject(Etapa10Objetivos vo) => new(vo.Meta3Meses, vo.Meta1Ano, vo.ExperienciaPassadaMarketing, vo.ResultadoPrioritario);
}
