namespace PersonaScript.Modules.Scripts.Application.DTOs;

public sealed record StoryBlockDto(
    string Periodo,
    string HorarioSugestao,
    string GatilhoRotina,
    string TipoConteudo,
    string ExemploPratico,
    string ObjetivoConexao
);

public sealed record StoryPlanDto(
    Guid Id,
    Guid AnamneseId,
    Guid? PersonaDiagnosisId,
    string FrequenciaDiariaRecomendada,
    List<StoryBlockDto> BlocosHorarios,
    string DiretrizesHumanizacao,
    DateTimeOffset GeradoEm
);

public sealed record WeeklyEditorialPlanDto(
    int NumeroSemana,
    string TemaCentral,
    string PilarConteudo,
    string ObjetivoEstrategico,
    string SugestaoFormato,
    List<string> IdeiasConteudo
);

public sealed record NinetyDayCalendarDto(
    Guid Id,
    Guid AnamneseId,
    Guid? PersonaDiagnosisId,
    string ObjetivoTrimestral,
    List<WeeklyEditorialPlanDto> Semanas,
    DateTimeOffset GeradoEm
);

public sealed record ContentPlanResultDto(
    StoryPlanDto PlanoStories,
    NinetyDayCalendarDto Calendario90Dias
);
