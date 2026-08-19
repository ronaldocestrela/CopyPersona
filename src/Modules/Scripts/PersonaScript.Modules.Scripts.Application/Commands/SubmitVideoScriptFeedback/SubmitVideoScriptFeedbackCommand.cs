using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Commands.SubmitVideoScriptFeedback;

public sealed record SubmitVideoScriptFeedbackCommand(
    Guid ScriptId,
    ScriptFeedbackRating Rating,
    string? Notes = null
) : ICommand;
