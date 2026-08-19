using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Scripts.Application.Commands.RegenerateVideoScript;

public sealed record RegenerateVideoScriptCommand(
    Guid TargetScriptId,
    string FeedbackNotes
) : ICommand<Guid>;
