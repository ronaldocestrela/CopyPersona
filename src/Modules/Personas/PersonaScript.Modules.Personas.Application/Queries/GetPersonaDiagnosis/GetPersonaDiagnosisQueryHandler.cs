using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Personas.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Personas.Application.Queries.GetPersonaDiagnosis;

public sealed class GetPersonaDiagnosisQueryHandler : IQueryHandler<GetPersonaDiagnosisQuery, PersonaDiagnosisDto?>
{
    private readonly IPersonaDiagnosisRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetPersonaDiagnosisQueryHandler(IPersonaDiagnosisRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PersonaDiagnosisDto?>> Handle(GetPersonaDiagnosisQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<PersonaDiagnosisDto?>(PersonaScript.Modules.Personas.Domain.DomainErrors.Personas.TenantIdInvalido);
        }

        var diagnosis = await _repository.GetByTenantIdAsync(cancellationToken);
        if (diagnosis is null)
        {
            return Result.Success<PersonaDiagnosisDto?>(null);
        }

        return Result.Success<PersonaDiagnosisDto?>(PersonaDiagnosisDto.FromEntity(diagnosis));
    }
}
