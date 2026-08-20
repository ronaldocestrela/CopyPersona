using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Application.DTOs;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Queries.GetAllPlans;

public record GetAllPlansQuery : IQuery<IReadOnlyList<PlanDto>>;

public sealed class GetAllPlansQueryHandler(IPlanRepository planRepository) : IQueryHandler<GetAllPlansQuery, IReadOnlyList<PlanDto>>
{
    public async Task<Result<IReadOnlyList<PlanDto>>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken)
    {
        var plans = await planRepository.GetAllAsync(cancellationToken);

        var planDtos = plans.Select(p => new PlanDto(
            p.Id,
            p.PlanType,
            p.Name,
            p.Description,
            p.MonthlyPrice,
            p.YearlyPrice,
            p.MaxActivePersonas,
            p.MaxScriptsPerMonth,
            p.MaxAiAnalysesPerMonth,
            p.IsActive,
            p.StripePriceId
        )).ToList();

        return Result.Success<IReadOnlyList<PlanDto>>(planDtos);
    }
}
