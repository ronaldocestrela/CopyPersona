using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Commands.Compliance;

public record CreateCouncilRuleCommand(
    string CouncilAcronym,
    string CouncilName,
    string ResolutionNumber,
    string GuidelinesText,
    string Category,
    bool IsActive = true
) : ICommand<Guid>;

public sealed class CreateCouncilRuleCommandHandler : ICommandHandler<CreateCouncilRuleCommand, Guid>
{
    private readonly ICouncilRuleRepository _repository;

    public CreateCouncilRuleCommandHandler(ICouncilRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateCouncilRuleCommand command, CancellationToken cancellationToken)
    {
        var ruleResult = CouncilRule.Create(
            command.CouncilAcronym,
            command.CouncilName,
            command.ResolutionNumber,
            command.GuidelinesText,
            command.Category,
            command.IsActive);

        if (ruleResult.IsFailure)
            return Result.Failure<Guid>(ruleResult.Error);

        await _repository.AddAsync(ruleResult.Value, cancellationToken);
        return Result.Success(ruleResult.Value.Id);
    }
}
