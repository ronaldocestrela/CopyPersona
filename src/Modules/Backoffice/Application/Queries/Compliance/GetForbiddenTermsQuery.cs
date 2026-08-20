using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Queries.Compliance;

public record GetForbiddenTermsQuery(bool OnlyActive = false) : IQuery<IReadOnlyList<ForbiddenTermDto>>;

public sealed class GetForbiddenTermsQueryHandler : IQueryHandler<GetForbiddenTermsQuery, IReadOnlyList<ForbiddenTermDto>>
{
    private readonly IForbiddenTermRepository _repository;

    public GetForbiddenTermsQueryHandler(IForbiddenTermRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ForbiddenTermDto>>> Handle(GetForbiddenTermsQuery query, CancellationToken cancellationToken)
    {
        var terms = query.OnlyActive
            ? await _repository.GetAllActiveAsync(cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var dtos = terms.Select(t => new ForbiddenTermDto(
            t.Id,
            t.Term,
            t.Category,
            t.Severity,
            t.ReplacementSuggestion,
            t.Reasoning,
            t.IsActive,
            t.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<ForbiddenTermDto>>(dtos);
    }
}
