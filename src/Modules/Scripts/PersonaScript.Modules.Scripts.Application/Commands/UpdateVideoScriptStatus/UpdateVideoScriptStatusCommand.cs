using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Commands.UpdateVideoScriptStatus;

public sealed record UpdateVideoScriptStatusCommand(
    Guid ScriptId,
    VideoScriptStatus NovoStatus
) : ICommand;
