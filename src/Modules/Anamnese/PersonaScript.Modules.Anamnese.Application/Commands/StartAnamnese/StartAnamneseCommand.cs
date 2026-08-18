using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;

public record StartAnamneseCommand : ICommand<Guid>;
