using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;
using PersonaScript.Modules.Scripts.Application.DTOs;

namespace PersonaScript.Modules.Scripts.Application.Services;

public sealed class ContentPlanGenerator : IContentPlanGenerator
{
    private readonly ILLMProvider _llmProvider;
    private readonly IContentPlanPromptBuilder _promptBuilder;

    public ContentPlanGenerator(ILLMProvider llmProvider, IContentPlanPromptBuilder promptBuilder)
    {
        _llmProvider = llmProvider;
        _promptBuilder = promptBuilder;
    }

    public async Task<Result<ContentPlanLLMResponseDto>> GeneratePlanAsync(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis,
        CancellationToken cancellationToken = default)
    {
        var prompt = _promptBuilder.BuildPrompt(anamnese, diagnosis);

        try
        {
            var request = new LLMRequest
            {
                UserPrompt = prompt,
                ResponseFormatJson = true,
                Temperature = 0.7
            };

            var llmResult = await _llmProvider.CompleteStructuredAsync<ContentPlanLLMResponseDto>(request, cancellationToken);
            if (llmResult.IsSuccess && llmResult.Value is not null && IsValidResponse(llmResult.Value))
            {
                return Result.Success(llmResult.Value);
            }
        }
        catch
        {
            // Fallback heurístico inteligente para garantir resiliência
        }

        var fallback = BuildHeuristicFallback(anamnese, diagnosis);
        return Result.Success(fallback);
    }

    private static bool IsValidResponse(ContentPlanLLMResponseDto dto)
    {
        return dto.PlanoStories != null &&
               dto.PlanoStories.BlocosHorarios?.Count > 0 &&
               dto.Calendario90Dias != null &&
               dto.Calendario90Dias.Semanas?.Count > 0;
    }

    private static ContentPlanLLMResponseDto BuildHeuristicFallback(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis)
    {
        var nome = anamnese.Etapa1?.ComoGostaSerChamado ?? anamnese.Etapa1?.NomeCompleto ?? "Profissional";
        var obj3Meses = anamnese.Etapa10?.Meta3Meses ?? "Atração e Fidelização de Pacientes";
        var pilarDominante = diagnosis?.PilaresConteudo.FirstOrDefault()?.Nome ?? "Educacional";

        var storyBlocks = new List<StoryBlockLLMDto>
        {
            new StoryBlockLLMDto(
                "Manhã",
                "08:00",
                "Chegada na clínica / início dos atendimentos",
                "Bastidores & Preparação",
                "Mostrar a organização do consultório e café da manhã",
                "Humanização e proximidade"),
            new StoryBlockLLMDto(
                "Almoço",
                "12:30",
                "Pausa para almoço",
                "Dica Rápida / Caixa de Perguntas",
                "Responder uma dúvida frequente de paciente",
                "Autoridade e tirada de dúvidas"),
            new StoryBlockLLMDto(
                "Fim de Tarde",
                "17:30",
                "Encerramento dos atendimentos",
                "Reflexão ou Prova Social",
                "Agradecimento pelo dia e reflexão sobre caso marcante",
                "Conexão emocional e conversão")
        };

        var planoStories = new StoryPlanLLMResponseDto(
            "3 a 5 stories por dia divididos em 3 blocos estratégicos",
            storyBlocks,
            "Manter linguagem espontânea, mostrando a rotina real sem artificialidade.");

        var semanas = new List<WeeklyEditorialPlanLLMDto>();
        for (int i = 1; i <= 12; i++)
        {
            semanas.Add(new WeeklyEditorialPlanLLMDto(
                i,
                $"Tema da Semana {i}: Foco em {pilarDominante}",
                pilarDominante,
                "Aumento de autoridade e engajamento",
                "Vídeo Curto / Reels + Carrossel",
                new List<string>
                {
                    $"Post 1 (Segunda): Introdução ao tema da semana {i}",
                    $"Post 2 (Quarta): Estudo de caso ou mito comum",
                    $"Post 3 (Sexta): Chamada direta para consulta ou direct"
                }));
        }

        var calendario90Dias = new NinetyDayCalendarLLMResponseDto(
            obj3Meses,
            semanas);

        return new ContentPlanLLMResponseDto(planoStories, calendario90Dias);
    }
}
