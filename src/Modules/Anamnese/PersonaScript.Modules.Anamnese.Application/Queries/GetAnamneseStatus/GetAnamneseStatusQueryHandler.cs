using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStatus;

public sealed class GetAnamneseStatusQueryHandler : IQueryHandler<GetAnamneseStatusQuery, AnamneseStatusDto>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetAnamneseStatusQueryHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<AnamneseStatusDto>> Handle(GetAnamneseStatusQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<AnamneseStatusDto>(DomainErrors.Anamnese.TenantIdInvalido);
        }

        var anamnese = await _repository.GetByTenantIdAsync(cancellationToken);
        if (anamnese is null)
        {
            return Result.Failure<AnamneseStatusDto>(DomainErrors.Anamnese.NaoEncontrada);
        }

        var dto = new AnamneseStatusDto(
            anamnese.Id,
            anamnese.Status,
            anamnese.EtapaAtual,
            anamnese.PercentualConclusao,
            anamnese.CriadoEm,
            anamnese.AtualizadoEm,
            anamnese.ConcluidoEm
        );

        return Result.Success(dto);
    }
}
