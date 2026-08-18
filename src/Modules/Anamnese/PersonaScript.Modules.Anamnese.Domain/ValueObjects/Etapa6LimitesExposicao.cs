namespace PersonaScript.Modules.Anamnese.Domain.ValueObjects;

public sealed record Etapa6LimitesExposicao(
    string AssuntosProibidos,
    string VidaPessoalAceita,
    string EstiloVidaAceito,
    string TrabalhoAceito,
    NivelConfortoCameraEnum NivelConfortoCamera,
    string RegrasConselhoRegional
);
