using System.Text.Json.Serialization;

namespace PersonaScript.Modules.Scripts.Application.DTOs;

public sealed record WeeklyEditorialPlanLLMDto(
    [property: JsonPropertyName("numero_semana")] int NumeroSemana,
    [property: JsonPropertyName("tema_central")] string TemaCentral,
    [property: JsonPropertyName("pilar_conteudo")] string PilarConteudo,
    [property: JsonPropertyName("objetivo_estrategico")] string ObjetivoEstrategico,
    [property: JsonPropertyName("sugestao_formato")] string SugestaoFormato,
    [property: JsonPropertyName("ideias_conteudo")] List<string> IdeiasConteudo
);

public sealed record NinetyDayCalendarLLMResponseDto(
    [property: JsonPropertyName("objetivo_trimestral")] string ObjetivoTrimestral,
    [property: JsonPropertyName("semanas")] List<WeeklyEditorialPlanLLMDto> Semanas
);

public sealed record ContentPlanLLMResponseDto(
    [property: JsonPropertyName("plano_stories")] StoryPlanLLMResponseDto PlanoStories,
    [property: JsonPropertyName("calendario_90_dias")] NinetyDayCalendarLLMResponseDto Calendario90Dias
);
