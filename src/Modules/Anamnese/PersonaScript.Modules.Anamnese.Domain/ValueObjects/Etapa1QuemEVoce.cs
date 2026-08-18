namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa1QuemEVoce(
    string NomeCompleto,
    string ComoGostaSerChamado,
    string ProfissaoEspecialidade,
    int TempoAtuacaoAnos,
    string FormacoesEspecializacoes,
    string PremiosTitulos,
    int PacientesMes,
    MomentoAtualEnum MomentoAtual
);
