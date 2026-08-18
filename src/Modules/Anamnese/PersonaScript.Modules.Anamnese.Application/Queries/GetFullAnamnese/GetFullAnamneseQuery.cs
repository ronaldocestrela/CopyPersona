using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;

public record GetFullAnamneseQuery : IQuery<FullAnamneseDto>;
