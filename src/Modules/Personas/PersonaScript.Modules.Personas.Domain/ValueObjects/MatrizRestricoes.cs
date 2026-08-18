namespace PersonaScript.Modules.Personas.Domain.ValueObjects;

public sealed record MatrizRestricoes(
    IReadOnlyCollection<string> TemasProibidos,
    IReadOnlyCollection<string> PalavrasEvitar,
    IReadOnlyCollection<string> DiretrizesInegociaveis,
    string LimitesExposicao
);
