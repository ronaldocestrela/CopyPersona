using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Personas.Domain.ValueObjects;

namespace PersonaScript.Modules.Personas.Application.DTOs;

public sealed record IdentidadeMarcaDto(
    string TomDeVoz,
    string EstiloVisualSugerido,
    string ArquetipoPrincipal,
    string ArquetipoSecundario
)
{
    public static IdentidadeMarcaDto FromValueObject(IdentidadeMarca vo)
        => new(vo.TomDeVoz, vo.EstiloVisualSugerido, vo.ArquetipoPrincipal, vo.ArquetipoSecundario);
}

public sealed record PilarConteudoDto(
    string Nome,
    int Percentual,
    string Descricao,
    IReadOnlyCollection<string> ExemplosTopicos
)
{
    public static PilarConteudoDto FromValueObject(PilarConteudo vo)
        => new(vo.Nome, vo.Percentual, vo.Descricao, vo.ExemplosTopicos);
}

public sealed record MatrizRestricoesDto(
    IReadOnlyCollection<string> TemasProibidos,
    IReadOnlyCollection<string> PalavrasEvitar,
    IReadOnlyCollection<string> DiretrizesInegociaveis,
    string LimitesExposicao
)
{
    public static MatrizRestricoesDto FromValueObject(MatrizRestricoes vo)
        => new(vo.TemasProibidos, vo.PalavrasEvitar, vo.DiretrizesInegociaveis, vo.LimitesExposicao);
}

public sealed record PersonaDiagnosisDto(
    Guid Id,
    Guid TenantId,
    Guid AnamneseId,
    string FrasePosicionamento,
    string SintesePerfil,
    IdentidadeMarcaDto IdentidadeMarca,
    IReadOnlyCollection<PilarConteudoDto> PilaresConteudo,
    MatrizRestricoesDto MatrizRestricoes,
    DateTimeOffset GeradoEm,
    DateTimeOffset? AtualizadoEm
)
{
    public static PersonaDiagnosisDto FromEntity(PersonaDiagnosis entity)
        => new(
            entity.Id,
            entity.TenantId,
            entity.AnamneseId,
            entity.FrasePosicionamento,
            entity.SintesePerfil,
            IdentidadeMarcaDto.FromValueObject(entity.IdentidadeMarca),
            entity.PilaresConteudo.Select(PilarConteudoDto.FromValueObject).ToList(),
            MatrizRestricoesDto.FromValueObject(entity.MatrizRestricoes),
            entity.GeradoEm,
            entity.AtualizadoEm
        );
}
