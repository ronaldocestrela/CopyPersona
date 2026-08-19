using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.DTOs;

public sealed record VideoScriptDto(
    Guid Id,
    Guid TenantId,
    Guid AnamneseId,
    Guid? PersonaDiagnosisId,
    string Tema,
    string PilarConteudo,
    string Objetivo,
    string Gancho,
    string Retencao,
    string ChamadaParaAcao,
    string LegendaSugerida,
    string DicasGravacao,
    string TomVozAplicado,
    VideoScriptStatus Status,
    DateTimeOffset GeradoEm,
    DateTimeOffset? AtualizadoEm);
