using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStep;

public record GetAnamneseStepQuery(int Etapa) : IQuery<object?>;
