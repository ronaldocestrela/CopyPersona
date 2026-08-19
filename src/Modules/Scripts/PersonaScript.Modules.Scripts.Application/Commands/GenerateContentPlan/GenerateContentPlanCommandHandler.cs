using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.BuildingBlocks.Tenancy;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Anamnese.Application.Queries.GetFullAnamnese;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.DTOs;
using PersonaScript.Modules.Scripts.Application.Services;
using PersonaScript.Modules.Scripts.Domain;
using PersonaScript.Modules.Scripts.Domain.ValueObjects;
using ScriptDomainErrors = PersonaScript.Modules.Scripts.Domain.DomainErrors;

namespace PersonaScript.Modules.Scripts.Application.Commands.GenerateContentPlan;

public sealed class GenerateContentPlanCommandHandler : ICommandHandler<GenerateContentPlanCommand, ContentPlanResultDto>
{
    private readonly IStoryPlanRepository _storyPlanRepository;
    private readonly INinetyDayCalendarRepository _calendarRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> _getFullAnamneseHandler;
    private readonly IPersonaDiagnosisRepository _personaDiagnosisRepository;
    private readonly IContentPlanGenerator _contentPlanGenerator;

    public GenerateContentPlanCommandHandler(
        IStoryPlanRepository storyPlanRepository,
        INinetyDayCalendarRepository calendarRepository,
        ITenantContext tenantContext,
        IQueryHandler<GetFullAnamneseQuery, FullAnamneseDto> getFullAnamneseHandler,
        IPersonaDiagnosisRepository personaDiagnosisRepository,
        IContentPlanGenerator contentPlanGenerator)
    {
        _storyPlanRepository = storyPlanRepository;
        _calendarRepository = calendarRepository;
        _tenantContext = tenantContext;
        _getFullAnamneseHandler = getFullAnamneseHandler;
        _personaDiagnosisRepository = personaDiagnosisRepository;
        _contentPlanGenerator = contentPlanGenerator;
    }

    public async Task<Result<ContentPlanResultDto>> Handle(GenerateContentPlanCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId.Value;
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<ContentPlanResultDto>(ScriptDomainErrors.Scripts.TenantIdInvalido);
        }

        // 1. Obter Anamnese do Tenant
        var anamneseResult = await _getFullAnamneseHandler.Handle(new GetFullAnamneseQuery(), cancellationToken);
        if (anamneseResult.IsFailure || anamneseResult.Value is null)
        {
            return Result.Failure<ContentPlanResultDto>(ScriptDomainErrors.Scripts.AnamneseOuDiagnosticoNaoEncontrado);
        }

        var anamnese = anamneseResult.Value;

        // 2. Obter Diagnóstico de Persona (opcional mas recomendado)
        var diagnosis = await _personaDiagnosisRepository.GetByTenantIdAsync(cancellationToken);

        // 3. Gerar via Agente 2 / LLM Provider
        var genResult = await _contentPlanGenerator.GeneratePlanAsync(anamnese, diagnosis, cancellationToken);
        if (genResult.IsFailure || genResult.Value is null)
        {
            return Result.Failure<ContentPlanResultDto>(ScriptDomainErrors.Scripts.FalhaGeracaoLLM);
        }

        var llmOutput = genResult.Value;

        // 4. Mapear e criar Entidade StoryPlan
        var storyBlocks = llmOutput.PlanoStories.BlocosHorarios.Select(b => new StoryBlock(
            b.Periodo,
            b.HorarioSugestao,
            b.GatilhoRotina,
            b.TipoConteudo,
            b.ExemploPratico,
            b.ObjetivoConexao
        )).ToList();

        var storyPlanResult = StoryPlan.Create(
            tenantId,
            anamnese.Status.Id,
            diagnosis?.Id,
            llmOutput.PlanoStories.FrequenciaDiariaRecomendada,
            storyBlocks,
            llmOutput.PlanoStories.DiretrizesHumanizacao);

        if (storyPlanResult.IsFailure)
        {
            return Result.Failure<ContentPlanResultDto>(storyPlanResult.Error);
        }

        // 5. Mapear e criar Entidade NinetyDayCalendar
        var semanas = llmOutput.Calendario90Dias.Semanas.Select(s => new WeeklyEditorialPlan(
            s.NumeroSemana,
            s.TemaCentral,
            s.PilarConteudo,
            s.ObjetivoEstrategico,
            s.SugestaoFormato,
            s.IdeiasConteudo
        )).ToList();

        var calendarResult = NinetyDayCalendar.Create(
            tenantId,
            anamnese.Status.Id,
            diagnosis?.Id,
            llmOutput.Calendario90Dias.ObjetivoTrimestral,
            semanas);

        if (calendarResult.IsFailure)
        {
            return Result.Failure<ContentPlanResultDto>(calendarResult.Error);
        }

        var storyPlan = storyPlanResult.Value;
        var calendar = calendarResult.Value;

        // 6. Persistir entidades no repositório do módulo
        await _storyPlanRepository.AddAsync(storyPlan, cancellationToken);
        await _calendarRepository.AddAsync(calendar, cancellationToken);

        // 7. Retornar DTO de resultado
        var storyPlanDto = new StoryPlanDto(
            storyPlan.Id,
            storyPlan.AnamneseId,
            storyPlan.PersonaDiagnosisId,
            storyPlan.FrequenciaDiariaRecomendada,
            storyPlan.BlocosHorarios.Select(b => new StoryBlockDto(
                b.Periodo, b.HorarioSugestao, b.GatilhoRotina, b.TipoConteudo, b.ExemploPratico, b.ObjetivoConexao
            )).ToList(),
            storyPlan.DiretrizesHumanizacao,
            storyPlan.GeradoEm);

        var calendarDto = new NinetyDayCalendarDto(
            calendar.Id,
            calendar.AnamneseId,
            calendar.PersonaDiagnosisId,
            calendar.ObjetivoTrimestral,
            calendar.Semanas.Select(s => new WeeklyEditorialPlanDto(
                s.NumeroSemana, s.TemaCentral, s.PilarConteudo, s.ObjetivoEstrategico, s.SugestaoFormato, s.IdeiasConteudo
            )).ToList(),
            calendar.GeradoEm);

        return Result.Success(new ContentPlanResultDto(storyPlanDto, calendarDto));
    }
}
