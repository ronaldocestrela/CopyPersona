using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Commands.StartAnamnese;

public sealed class StartAnamneseCommandHandler : ICommandHandler<StartAnamneseCommand, Guid>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public StartAnamneseCommandHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(StartAnamneseCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(DomainErrors.Anamnese.TenantIdInvalido);
        }

        var existing = await _repository.GetByTenantIdAsync(cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == AnamneseStatus.Concluido)
            {
                return Result.Failure<Guid>(DomainErrors.Anamnese.JaConcluida);
            }

            return Result.Success(existing.Id);
        }

        var createResult = Domain.Anamnese.Create(tenantId);
        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        var anamnese = createResult.Value;
        await _repository.AddAsync(anamnese, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(anamnese.Id);
    }
}
