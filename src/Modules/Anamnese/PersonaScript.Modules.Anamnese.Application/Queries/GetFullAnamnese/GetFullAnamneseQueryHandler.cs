using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;

public sealed class GetFullAnamneseQueryHandler : IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetFullAnamneseQueryHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<FullAnamneseDto>> Handle(GetFullAnamneseQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<FullAnamneseDto>(DomainErrors.Anamnese.TenantIdInvalido);
        }

        var anamnese = await _repository.GetByTenantIdAsync(cancellationToken);
        if (anamnese is null)
        {
            return Result.Failure<FullAnamneseDto>(DomainErrors.Anamnese.NaoEncontrada);
        }

        var statusDto = new AnamneseStatusDto(
            anamnese.Id,
            anamnese.Status,
            anamnese.EtapaAtual,
            anamnese.PercentualConclusao,
            anamnese.CriadoEm,
            anamnese.AtualizadoEm,
            anamnese.ConcluidoEm
        );

        var fullDto = new FullAnamneseDto(
            statusDto,
            anamnese.Etapa1 is null ? null : Etapa1Dto.FromValueObject(anamnese.Etapa1),
            anamnese.Etapa2 is null ? null : Etapa2Dto.FromValueObject(anamnese.Etapa2),
            anamnese.Etapa3 is null ? null : Etapa3Dto.FromValueObject(anamnese.Etapa3),
            anamnese.Etapa4 is null ? null : Etapa4Dto.FromValueObject(anamnese.Etapa4),
            anamnese.Etapa5 is null ? null : Etapa5Dto.FromValueObject(anamnese.Etapa5),
            anamnese.Etapa6 is null ? null : Etapa6Dto.FromValueObject(anamnese.Etapa6),
            anamnese.Etapa7 is null ? null : Etapa7Dto.FromValueObject(anamnese.Etapa7),
            anamnese.Etapa8 is null ? null : Etapa8Dto.FromValueObject(anamnese.Etapa8),
            anamnese.Etapa9 is null ? null : Etapa9Dto.FromValueObject(anamnese.Etapa9),
            anamnese.Etapa10 is null ? null : Etapa10Dto.FromValueObject(anamnese.Etapa10)
        );

        return Result.Success(fullDto);
    }
}
