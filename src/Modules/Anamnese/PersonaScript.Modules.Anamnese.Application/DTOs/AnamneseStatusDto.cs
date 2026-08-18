using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.DTOs;

public record AnamneseStatusDto(
    Guid Id,
    AnamneseStatus Status,
    int EtapaAtual,
    int PercentualConclusao,
    DateTimeOffset CriadoEm,
    DateTimeOffset? AtualizadoEm,
    DateTimeOffset? ConcluidoEm
);
