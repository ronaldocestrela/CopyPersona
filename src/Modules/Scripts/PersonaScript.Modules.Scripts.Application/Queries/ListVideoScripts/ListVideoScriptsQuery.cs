using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Queries.ListVideoScripts;

public sealed record ListVideoScriptsQuery(
    VideoScriptStatus? Status = null,
    string? SearchTerm = null,
    string? PilarConteudo = null
) : IQuery<IReadOnlyList<VideoScriptDto>>;
