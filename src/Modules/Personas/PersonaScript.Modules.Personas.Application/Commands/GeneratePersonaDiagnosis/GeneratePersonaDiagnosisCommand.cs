using PersonaScript.BuildingBlocks.CQRS;

namespace PersonaScript.Modules.Personas.Application.Commands.GeneratePersonaDiagnosis;

public sealed record GeneratePersonaDiagnosisCommand : ICommand<Guid>;
