using PersonaScript.BuildingBlocks.AI.Abstractions;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Services;

public sealed class PersonaDiagnosisGenerator : IPersonaDiagnosisGenerator
{
    private readonly ILLMProvider _llmProvider;
    private readonly IPersonaPromptBuilder _promptBuilder;

    public PersonaDiagnosisGenerator(ILLMProvider llmProvider, IPersonaPromptBuilder promptBuilder)
    {
        _llmProvider = llmProvider;
        _promptBuilder = promptBuilder;
    }

    public async Task<Result<PersonaDiagnosisLLMResponseDto>> GenerateAsync(
        FullAnamneseDto anamnese,
        string? feedback = null,
        CancellationToken cancellationToken = default)
    {
        var request = _promptBuilder.BuildPrompt(anamnese, feedback);

        try
        {
            var llmResult = await _llmProvider.CompleteStructuredAsync<PersonaDiagnosisLLMResponseDto>(request, cancellationToken);
            if (llmResult.IsSuccess && llmResult.Value is not null)
            {
                var normalized = NormalizePilares(llmResult.Value);
                return Result.Success(normalized);
            }
        }
        catch
        {
            // Fallback to heuristic generation below
        }

        var fallbackResponse = BuildHeuristicFallback(anamnese);
        return Result.Success(fallbackResponse);
    }

    private static PersonaDiagnosisLLMResponseDto NormalizePilares(PersonaDiagnosisLLMResponseDto dto)
    {
        if (dto.PilaresConteudo == null || dto.PilaresConteudo.Count == 0)
        {
            return dto with { PilaresConteudo = GetDefaultPilares() };
        }

        var total = dto.PilaresConteudo.Sum(p => p.Percentual);
        if (total == 100) return dto;

        // If not 100, recalculate proportionally
        var adjusted = dto.PilaresConteudo.Select(p => p with
        {
            Percentual = Math.Max(5, (int)Math.Round((double)p.Percentual / total * 100))
        }).ToList();

        var newTotal = adjusted.Sum(p => p.Percentual);
        if (newTotal != 100 && adjusted.Count > 0)
        {
            var diff = 100 - newTotal;
            adjusted[0] = adjusted[0] with { Percentual = adjusted[0].Percentual + diff };
        }

        return dto with { PilaresConteudo = adjusted };
    }

    private static PersonaDiagnosisLLMResponseDto BuildHeuristicFallback(FullAnamneseDto anamnese)
    {
        var nome = anamnese.Etapa1?.ComoGostaSerChamado ?? anamnese.Etapa1?.NomeCompleto ?? "Profissional";
        var profissao = anamnese.Etapa1?.ProfissaoEspecialidade ?? "Especialista";
        var dor = anamnese.Etapa4?.MaioresMedos ?? "resolver suas principais dores e necessidades";

        var frase = $"Autoridade e referência em {profissao}, ajudando clientes a superarem {dor}.";
        var sintese = $"{nome} atua como {profissao} focado em resultados de excelência e atendimento humanizado. Seu público busca superação de {dor}.";

        var tom = anamnese.Etapa8?.ArquetiposComunicacao != null && anamnese.Etapa8.ArquetiposComunicacao.Count > 0
            ? string.Join(", ", anamnese.Etapa8.ArquetiposComunicacao)
            : "Profissional, acolhedor e fundamentado";

        var estilo = "Design elegante, minimalista e limpo";
        var rejeita = anamnese.Etapa5?.OQueNaoFariaArea ?? "Conteúdo apelativo ou sem embasamento";
        var limites = anamnese.Etapa6?.AssuntosProibidos ?? "Privacidade pessoal e familiar preservadas";

        return new PersonaDiagnosisLLMResponseDto
        {
            FrasePosicionamento = frase,
            SintesePerfil = sintese,
            TomDeVoz = tom,
            EstiloVisualSugerido = estilo,
            ArquetipoPrincipal = "O Sábio",
            ArquetipoSecundario = "O Herói",
            PilaresConteudo = GetDefaultPilares(),
            TemasProibidos = string.IsNullOrWhiteSpace(rejeita) ? new List<string> { "Conteúdo de baixa qualidade" } : new List<string> { rejeita },
            PalavrasEvitar = new List<string> { "Garantia milagrosa", "Desconto excessivo" },
            DiretrizesInegociaveis = new List<string> { "Ética profissional inegociável", "Respeito à privacidade do cliente" },
            LimitesExposicao = limites
        };
    }

    private static List<PilarLLMItemDto> GetDefaultPilares() => new()
    {
        new PilarLLMItemDto
        {
            Nome = "Educação & Esclarecimento",
            Percentual = 30,
            Descricao = "Explicar conceitos, desmistificar tabus e educar a audiência.",
            ExemplosTopicos = new List<string> { "Mitos e Verdades", "Guia Passo a Passo" }
        },
        new PilarLLMItemDto
        {
            Nome = "Prova & Bastidores",
            Percentual = 25,
            Descricao = "Demonstrar autoridade através de casos reais, depoimentos e bastidores.",
            ExemplosTopicos = new List<string> { "Bastidores do atendimento", "Transformações reais" }
        },
        new PilarLLMItemDto
        {
            Nome = "Autoridade & Posicionamento",
            Percentual = 25,
            Descricao = "Opinião técnica, análises críticas e diferenciais de atuação.",
            ExemplosTopicos = new List<string> { "Por que escolhi esse método", "Erros mais comuns" }
        },
        new PilarLLMItemDto
        {
            Nome = "Conexão & Valores",
            Percentual = 20,
            Descricao = "Valores de marca, visão de mundo e história pessoal.",
            ExemplosTopicos = new List<string> { "Principais aprendizados", "Minha missão de vida" }
        }
    };
}
