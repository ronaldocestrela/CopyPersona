namespace PersonaScript.Modules.Scripts.Domain.ValueObjects;

public sealed record StoryBlock
{
    public string Periodo { get; init; } = string.Empty;
    public string HorarioSugestao { get; init; } = string.Empty;
    public string GatilhoRotina { get; init; } = string.Empty;
    public string TipoConteudo { get; init; } = string.Empty;
    public string ExemploPratico { get; init; } = string.Empty;
    public string ObjetivoConexao { get; init; } = string.Empty;

    public StoryBlock() { } // EF Core / JSON Constructor

    public StoryBlock(
        string periodo,
        string horarioSugestao,
        string gatilhoRotina,
        string tipoConteudo,
        string exemploPratico,
        string objetivoConexao)
    {
        Periodo = periodo;
        HorarioSugestao = horarioSugestao;
        GatilhoRotina = gatilhoRotina;
        TipoConteudo = tipoConteudo;
        ExemploPratico = exemploPratico;
        ObjetivoConexao = objetivoConexao;
    }
}
