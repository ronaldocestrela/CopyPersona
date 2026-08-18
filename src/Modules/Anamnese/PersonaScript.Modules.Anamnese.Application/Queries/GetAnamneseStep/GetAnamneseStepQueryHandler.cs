using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Queries.GetAnamneseStep;

public sealed class GetAnamneseStepQueryHandler : IQueryHandler<GetAnamneseStepQuery, object?>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetAnamneseStepQueryHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<object?>> Handle(GetAnamneseStepQuery query, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<object?>(DomainErrors.Anamnese.TenantIdInvalido);
        }

        if (query.Etapa is < 1 or > 10)
        {
            return Result.Failure<object?>(DomainErrors.Anamnese.EtapaInvalida);
        }

        var anamnese = await _repository.GetByTenantIdAsync(cancellationToken);
        if (anamnese is null)
        {
            return Result.Failure<object?>(DomainErrors.Anamnese.NaoEncontrada);
        }

        object? stepDto = query.Etapa switch
        {
            1 => anamnese.Etapa1 is null ? null : Etapa1Dto.FromValueObject(anamnese.Etapa1),
            2 => anamnese.Etapa2 is null ? null : Etapa2Dto.FromValueObject(anamnese.Etapa2),
            3 => anamnese.Etapa3 is null ? null : Etapa3Dto.FromValueObject(anamnese.Etapa3),
            4 => anamnese.Etapa4 is null ? null : Etapa4Dto.FromValueObject(anamnese.Etapa4),
            5 => anamnese.Etapa5 is null ? null : Etapa5Dto.FromValueObject(anamnese.Etapa5),
            6 => anamnese.Etapa6 is null ? null : Etapa6Dto.FromValueObject(anamnese.Etapa6),
            7 => anamnese.Etapa7 is null ? null : Etapa7Dto.FromValueObject(anamnese.Etapa7),
            8 => anamnese.Etapa8 is null ? null : Etapa8Dto.FromValueObject(anamnese.Etapa8),
            9 => anamnese.Etapa9 is null ? null : Etapa9Dto.FromValueObject(anamnese.Etapa9),
            10 => anamnese.Etapa10 is null ? null : Etapa10Dto.FromValueObject(anamnese.Etapa10),
            _ => null
        };

        return Result.Success(stepDto);
    }
}
