using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Domain;

namespace PersonaScript.Modules.Scripts.Application.Queries.GetNinetyDayCalendar;

public sealed record GetNinetyDayCalendarQuery : IQuery<NinetyDayCalendarDto>;

public sealed class GetNinetyDayCalendarQueryHandler : IQueryHandler<GetNinetyDayCalendarQuery, NinetyDayCalendarDto>
{
    private readonly INinetyDayCalendarRepository _repository;
    private readonly ITenantContext _tenantContext;

    public GetNinetyDayCalendarQueryHandler(INinetyDayCalendarRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<NinetyDayCalendarDto>> Handle(GetNinetyDayCalendarQuery query, CancellationToken cancellationToken)
    {
        if (_tenantContext.TenantId.Value == Guid.Empty)
        {
            return Result.Failure<NinetyDayCalendarDto>(DomainErrors.Scripts.TenantIdInvalido);
        }

        var calendar = await _repository.GetByTenantIdAsync(cancellationToken);
        if (calendar is null)
        {
            return Result.Failure<NinetyDayCalendarDto>(DomainErrors.Scripts.NinetyDayCalendarNaoEncontrado);
        }

        var dto = new NinetyDayCalendarDto(
            calendar.Id,
            calendar.AnamneseId,
            calendar.PersonaDiagnosisId,
            calendar.ObjetivoTrimestral,
            calendar.Semanas.Select(s => new WeeklyEditorialPlanDto(
                s.NumeroSemana, s.TemaCentral, s.PilarConteudo, s.ObjetivoEstrategico, s.SugestaoFormato, s.IdeiasConteudo
            )).ToList(),
            calendar.GeradoEm);

        return Result.Success(dto);
    }
}
