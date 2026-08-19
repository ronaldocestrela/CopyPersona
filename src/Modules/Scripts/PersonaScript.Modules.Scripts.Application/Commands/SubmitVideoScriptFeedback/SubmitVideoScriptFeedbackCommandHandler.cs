using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Commands.SubmitVideoScriptFeedback;

public sealed class SubmitVideoScriptFeedbackCommandHandler : ICommandHandler<SubmitVideoScriptFeedbackCommand>
{
    private readonly IVideoScriptRepository _repository;
    private readonly ITenantContext _tenantContext;

    public SubmitVideoScriptFeedbackCommandHandler(IVideoScriptRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(SubmitVideoScriptFeedbackCommand command, CancellationToken cancellationToken)
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

        var feedbackResult = script.RegisterFeedback(command.Rating, command.Notes);
        if (feedbackResult.IsFailure)
        {
            return feedbackResult;
        }

        await _repository.UpdateAsync(script, cancellationToken);
        return Result.Success();
    }
}
