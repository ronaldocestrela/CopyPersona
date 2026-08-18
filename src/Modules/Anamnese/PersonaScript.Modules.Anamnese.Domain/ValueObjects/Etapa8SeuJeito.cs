namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa8SeuJeito(
    IReadOnlyCollection<ArquetipoComunicacaoEnum> ArquetiposComunicacao,
    string AmostraEscritaExplicativa,
    string IdentidadeVisualStatus,
    string EsteticaOdiada
);
