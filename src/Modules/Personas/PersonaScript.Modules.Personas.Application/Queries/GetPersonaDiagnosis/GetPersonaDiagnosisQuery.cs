using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Personas.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;

public sealed record GetPersonaDiagnosisQuery : IQuery<PersonaDiagnosisDto?>;
