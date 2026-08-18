using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Domain;

namespace PersonaScript.Modules.Anamnese.Application.Commands.CompleteAnamnese;

public sealed class CompleteAnamneseCommandHandler : ICommandHandler<CompleteAnamneseCommand>
{
    private readonly IAnamneseRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CompleteAnamneseCommandHandler(IAnamneseRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(CompleteAnamneseCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure(DomainErrors.Anamnese.TenantIdInvalido);
        }

        var anamnese = await _repository.GetByTenantIdAsync(cancellationToken);
        if (anamnese is null)
        {
            return Result.Failure(DomainErrors.Anamnese.NaoEncontrada);
        }

        var result = anamnese.Concluir();
        if (result.IsFailure)
        {
            return result;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
