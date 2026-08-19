using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Scripts.Application.DTOs;

namespace PersonaScript.Modules.Scripts.Application.Queries.GetVideoScriptById;

public sealed record GetVideoScriptByIdQuery(Guid ScriptId) : IQuery<VideoScriptDto>;
