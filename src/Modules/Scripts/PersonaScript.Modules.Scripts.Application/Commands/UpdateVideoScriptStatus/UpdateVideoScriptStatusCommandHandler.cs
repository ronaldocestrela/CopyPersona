using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Commands.UpdateVideoScriptStatus;

public sealed class UpdateVideoScriptStatusCommandHandler : ICommandHandler<UpdateVideoScriptStatusCommand>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;

    public UpdateVideoScriptStatusCommandHandler(IVideoScriptRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateVideoScriptStatusCommand command, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure(DomainErrors.Scripts.TenantIdInvalido);
        }

        var script = await _repository.GetByIdAsync(command.ScriptId, cancellationToken);
        if (script is null)
        {
            return Result.Failure(DomainErrors.Scripts.ScriptNaoEncontrado);
        }

        var updateResult = script.UpdateStatus(command.NovoStatus);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        _repository.Update(script);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
