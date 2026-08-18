namespace PersonaScript.Modules.Personas.Domain.ValueObjects;

public sealed record PilarConteudo(
    string Nome,
    int Percentual,
    string Descricao,
    IReadOnlyCollection<string> ExemplosTopicos
);
