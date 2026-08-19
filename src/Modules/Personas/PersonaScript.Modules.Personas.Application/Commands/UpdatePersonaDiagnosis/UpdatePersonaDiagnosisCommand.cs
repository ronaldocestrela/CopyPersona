using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.Modules.Personas.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Commands.UpdatePersonaDiagnosis;

public sealed record UpdatePersonaDiagnosisCommand(
    string FrasePosicionamento,
    string SintesePerfil,
    IdentidadeMarcaDto IdentidadeMarca,
    IReadOnlyCollection<PilarConteudoDto> PilaresConteudo,
    MatrizRestricoesDto MatrizRestricoes
) : ICommand<Guid>;
