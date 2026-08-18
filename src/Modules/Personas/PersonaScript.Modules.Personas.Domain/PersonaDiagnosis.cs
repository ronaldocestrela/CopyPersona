using PersonaScript.BuildingBlocks.Domain;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Personas.Domain.ValueObjects;

namespace PersonaScript.Modules.Personas.Domain;

public sealed class PersonaDiagnosis : BaseEntity, IMustHaveTenant
{
    private readonly List<PilarConteudo> _pilaresConteudo = new();

    private PersonaDiagnosis() { } // EF Core constructor

    private PersonaDiagnosis(
        Guid tenantId,
        Guid anamneseId,
        string frasePosicionamento,
        string sintesePerfil,
        IdentidadeMarca identidadeMarca,
        IEnumerable<PilarConteudo> pilaresConteudo,
        MatrizRestricoes matrizRestricoes)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        AnamneseId = anamneseId;
        FrasePosicionamento = frasePosicionamento;
        SintesePerfil = sintesePerfil;
        IdentidadeMarca = identidadeMarca;
        _pilaresConteudo.AddRange(pilaresConteudo);
        MatrizRestricoes = matrizRestricoes;
        GeradoEm = DateTimeOffset.UtcNow;
    }

    public Guid TenantId { get; private set; }
    public Guid AnamneseId { get; private set; }
    public string FrasePosicionamento { get; private set; } = string.Empty;
    public string SintesePerfil { get; private set; } = string.Empty;
    public IdentidadeMarca IdentidadeMarca { get; private set; } = null!;
    public IReadOnlyCollection<PilarConteudo> PilaresConteudo => _pilaresConteudo.AsReadOnly();
    public MatrizRestricoes MatrizRestricoes { get; private set; } = null!;
    public DateTimeOffset GeradoEm { get; private set; }
    public DateTimeOffset? AtualizadoEm { get; private set; }

    public void SetTenantId(Guid tenantId)
    {
        TenantId = tenantId;
    }

    public static Result<PersonaDiagnosis> Create(
        Guid tenantId,
        Guid anamneseId,
        string frasePosicionamento,
        string sintesePerfil,
        IdentidadeMarca identidadeMarca,
        IEnumerable<PilarConteudo> pilaresConteudo,
        MatrizRestricoes matrizRestricoes)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<PersonaDiagnosis>(DomainErrors.Personas.TenantIdInvalido);
        }

        if (anamneseId == Guid.Empty)
        {
            return Result.Failure<PersonaDiagnosis>(DomainErrors.Personas.AnamneseIdInvalida);
        }

        var pilaresList = pilaresConteudo.ToList();
        var somaPercentual = pilaresList.Sum(p => p.Percentual);
        if (somaPercentual != 100)
        {
            return Result.Failure<PersonaDiagnosis>(DomainErrors.Personas.PercentualPilaresInvalido);
        }

        return Result.Success(new PersonaDiagnosis(
            tenantId,
            anamneseId,
            frasePosicionamento,
            sintesePerfil,
            identidadeMarca,
            pilaresList,
            matrizRestricoes
        ));
    }

    public Result Update(
        string frasePosicionamento,
        string sintesePerfil,
        IdentidadeMarca identidadeMarca,
        IEnumerable<PilarConteudo> pilaresConteudo,
        MatrizRestricoes matrizRestricoes)
    {
        var pilaresList = pilaresConteudo.ToList();
        var somaPercentual = pilaresList.Sum(p => p.Percentual);
        if (somaPercentual != 100)
        {
            return Result.Failure(DomainErrors.Personas.PercentualPilaresInvalido);
        }

        FrasePosicionamento = frasePosicionamento;
        SintesePerfil = sintesePerfil;
        IdentidadeMarca = identidadeMarca;
        _pilaresConteudo.Clear();
        _pilaresConteudo.AddRange(pilaresList);
        MatrizRestricoes = matrizRestricoes;
        AtualizadoEm = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
