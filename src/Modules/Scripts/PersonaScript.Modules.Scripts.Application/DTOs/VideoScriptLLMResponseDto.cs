namespace PersonaScript.Modules.Scripts.Application.DTOs;

public sealed record VideoScriptLLMResponseDto
{
    public string Gancho { get; init; } = string.Empty;
    public string Retencao { get; init; } = string.Empty;
    public string ChamadaParaAcao { get; init; } = string.Empty;
    public string LegendaSugerida { get; init; } = string.Empty;
    public string DicasGravacao { get; init; } = string.Empty;
    public string TomVozAplicado { get; init; } = string.Empty;
}
