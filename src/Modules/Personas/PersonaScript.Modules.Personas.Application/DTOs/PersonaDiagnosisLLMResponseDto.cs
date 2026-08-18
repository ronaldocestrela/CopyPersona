namespace PersonaScript.Modules.Personas.Application.DTOs;

public sealed record PilarLLMItemDto
{
    public required string Nome { get; init; }
    public required int Percentual { get; init; }
    public required string Descricao { get; init; }
    public required List<string> ExemplosTopicos { get; init; } = new();
}

public sealed record PersonaDiagnosisLLMResponseDto
{
    public required string FrasePosicionamento { get; init; }
    public required string SintesePerfil { get; init; }
    public required string TomDeVoz { get; init; }
    public required string EstiloVisualSugerido { get; init; }
    public required string ArquetipoPrincipal { get; init; }
    public required string ArquetipoSecundario { get; init; }
    public required List<PilarLLMItemDto> PilaresConteudo { get; init; } = new();
    public required List<string> TemasProibidos { get; init; } = new();
    public required List<string> PalavrasEvitar { get; init; } = new();
    public required List<string> DiretrizesInegociaveis { get; init; } = new();
    public required string LimitesExposicao { get; init; } = string.Empty;
}
