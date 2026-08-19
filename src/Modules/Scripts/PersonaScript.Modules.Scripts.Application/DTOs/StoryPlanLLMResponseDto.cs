using System.Text.Json.Serialization;

namespace PersonaScript.Modules.Scripts.Application.DTOs;

public sealed record StoryBlockLLMDto(
    [property: JsonPropertyName("periodo")] string Periodo,
    [property: JsonPropertyName("horario_sugestao")] string HorarioSugestao,
    [property: JsonPropertyName("gatilho_rotina")] string GatilhoRotina,
    [property: JsonPropertyName("tipo_conteudo")] string TipoConteudo,
    [property: JsonPropertyName("exemplo_pratico")] string ExemploPratico,
    [property: JsonPropertyName("objetivo_conexao")] string ObjetivoConexao
);

public sealed record StoryPlanLLMResponseDto(
    [property: JsonPropertyName("frequencia_diaria_recomendada")] string FrequenciaDiariaRecomendada,
    [property: JsonPropertyName("blocos_horarios")] List<StoryBlockLLMDto> BlocosHorarios,
    [property: JsonPropertyName("diretrizes_humanizacao")] string DiretrizesHumanizacao
);
