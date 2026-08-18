using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;

public record GetAnamneseStatusQuery : IQuery<AnamneseStatusDto>;
