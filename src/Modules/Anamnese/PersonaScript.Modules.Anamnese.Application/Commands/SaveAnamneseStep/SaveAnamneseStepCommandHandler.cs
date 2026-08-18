using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Commands.SaveAnamneseStep;

public sealed class SaveAnamneseStepCommandHandler : ICommandHandler<SaveAnamneseStepCommand>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public SaveAnamneseStepCommandHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SaveAnamneseStepCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure(DomainErrors.Anamnese.TenantIdInvalido);
        }

        if (command.Etapa is < 1 or > 10)
        {
            return Result.Failure(DomainErrors.Anamnese.EtapaInvalida);
        }

        var anamnese = await _repository.GetByTenantIdAsync(cancellationToken);
        if (anamnese is null)
        {
            var createResult = Domain.Anamnese.Create(tenantId);
            if (createResult.IsFailure)
            {
                return createResult;
            }

            anamnese = createResult.Value;
            await _repository.AddAsync(anamnese, cancellationToken);
        }

        if (anamnese.Status == AnamneseStatus.Concluido)
        {
            return Result.Failure(DomainErrors.Anamnese.JaConcluida);
        }

        Result updateResult = command.Etapa switch
        {
            1 => command.Etapa1 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa1(command.Etapa1.ToValueObject()),
            2 => command.Etapa2 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa2(command.Etapa2.ToValueObject()),
            3 => command.Etapa3 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa3(command.Etapa3.ToValueObject()),
            4 => command.Etapa4 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa4(command.Etapa4.ToValueObject()),
            5 => command.Etapa5 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa5(command.Etapa5.ToValueObject()),
            6 => command.Etapa6 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa6(command.Etapa6.ToValueObject()),
            7 => command.Etapa7 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa7(command.Etapa7.ToValueObject()),
            8 => command.Etapa8 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa8(command.Etapa8.ToValueObject()),
            9 => command.Etapa9 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa9(command.Etapa9.ToValueObject()),
            10 => command.Etapa10 is null ? Result.Failure(DomainErrors.Anamnese.EtapaInvalida) : anamnese.UpdateEtapa10(command.Etapa10.ToValueObject()),
            _ => Result.Failure(DomainErrors.Anamnese.EtapaInvalida)
        };

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
