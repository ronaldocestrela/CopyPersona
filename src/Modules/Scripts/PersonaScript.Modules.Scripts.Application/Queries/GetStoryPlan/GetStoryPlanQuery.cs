using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Queries.GetStoryPlan;

public sealed record GetStoryPlanQuery : IQuery<StoryPlanDto>;

public sealed class GetStoryPlanQueryHandler : IQueryHandler<GetStoryPlanQuery, StoryPlanDto>
{
    private readonly IStoryPlanRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetStoryPlanQueryHandler(IStoryPlanRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<StoryPlanDto>> Handle(GetStoryPlanQuery query, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<StoryPlanDto>(DomainErrors.Scripts.TenantIdInvalido);
        }

        var plan = await _repository.GetByTenantIdAsync(cancellationToken);
        if (plan is null)
        {
            return Result.Failure<StoryPlanDto>(DomainErrors.Scripts.StoryPlanNaoEncontrado);
        }

        var dto = new StoryPlanDto(
            plan.Id,
            plan.AnamneseId,
            plan.PersonaDiagnosisId,
            plan.FrequenciaDiariaRecomendada,
            plan.BlocosHorarios.Select(b => new StoryBlockDto(
                b.Periodo, b.HorarioSugestao, b.GatilhoRotina, b.TipoConteudo, b.ExemploPratico, b.ObjetivoConexao
            )).ToList(),
            plan.DiretrizesHumanizacao,
            plan.GeradoEm);

        return Result.Success(dto);
    }
}
