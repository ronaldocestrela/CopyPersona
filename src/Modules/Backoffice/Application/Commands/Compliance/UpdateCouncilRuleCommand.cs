using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Compliance;

public record UpdateCouncilRuleCommand(
    Guid Id,
    string CouncilName,
    string ResolutionNumber,
    string GuidelinesText,
    string Category
) : ICommand;

public sealed class UpdateCouncilRuleCommandHandler : ICommandHandler<UpdateCouncilRuleCommand>
{
    private readonly ICouncilRuleRepository _repository;

    public UpdateCouncilRuleCommandHandler(ICouncilRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateCouncilRuleCommand command, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (rule == null)
            return Result.Failure(Error.NotFound("CouncilRule.NotFound", "Regra de conselho não encontrada."));

        rule.Update(command.CouncilName, command.ResolutionNumber, command.GuidelinesText, command.Category);
        await _repository.UpdateAsync(rule, cancellationToken);

        return Result.Success();
    }
}

public record ToggleCouncilRuleStatusCommand(Guid Id) : ICommand;

public sealed class ToggleCouncilRuleStatusCommandHandler : ICommandHandler<ToggleCouncilRuleStatusCommand>
{
    private readonly ICouncilRuleRepository _repository;

    public ToggleCouncilRuleStatusCommandHandler(ICouncilRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(ToggleCouncilRuleStatusCommand command, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (rule == null)
            return Result.Failure(Error.NotFound("CouncilRule.NotFound", "Regra de conselho não encontrada."));

        if (rule.IsActive)
            rule.Deactivate();
        else
            rule.Activate();

        await _repository.UpdateAsync(rule, cancellationToken);

        return Result.Success();
    }
}
