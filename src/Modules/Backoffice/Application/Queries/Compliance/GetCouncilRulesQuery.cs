using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Compliance;

public record GetCouncilRulesQuery(bool OnlyActive = false) : IQuery<IReadOnlyList<CouncilRuleDto>>;

public sealed class GetCouncilRulesQueryHandler : IQueryHandler<GetCouncilRulesQuery, IReadOnlyList<CouncilRuleDto>>
{
    private readonly ICouncilRuleRepository _repository;

    public GetCouncilRulesQueryHandler(ICouncilRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CouncilRuleDto>>> Handle(GetCouncilRulesQuery query, CancellationToken cancellationToken)
    {
        var rules = query.OnlyActive
            ? await _repository.GetAllActiveAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var dtos = rules.Select(r => new CouncilRuleDto(
            r.Id,
            r.CouncilAcronym,
            r.CouncilName,
            r.ResolutionNumber,
            r.GuidelinesText,
            r.Category,
            r.IsActive,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return Result.Success<IReadOnlyList<CouncilRuleDto>>(dtos);
    }
}
