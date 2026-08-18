using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Anamnese.Application.Services;

public class HeuristicClarificationAnalyzer
{
    public static readonly string[] GenericClichesEtapa3 = new[]
    {
        "humanizado", "dedicado", "atendo com amor", "atendimento bom", "sou bom", "qualidade", "preço justo", "carinho", "dedicação"
    };

    public static readonly string[] GenericClichesEtapa4 = new[]
    {
        "melhorar de vida", "ficar bem", "emagrecer", "tratar dor", "ajudar", "ser feliz", "saúde"
    };

    public static readonly string[] GenericClichesEtapa7 = new[]
    {
        "aplico técnicas", "curso x", "faço de tudo", "uso métodos", "estudo muito", "técnicas modernas"
    };

    public static readonly string[] GenericClichesEtapa8 = new[]
    {
        "falo normal", "sou calmo", "falo bem", "escrevo normal", "não sei"
    };

    public ClarificationAnalysisResultDto AnalyzeStep(int stepNumber, object stepDto)
    {
        var items = new List<ClarificationItemDto>();

        switch (stepNumber)
        {
            case 3 when stepDto is Etapa3Dto e3:
                AnalyzeEtapa3(e3, items);
                break;
            case 4 when stepDto is Etapa4Dto e4:
                AnalyzeEtapa4(e4, items);
                break;
            case 7 when stepDto is Etapa7Dto e7:
                AnalyzeEtapa7(e7, items);
                break;
            case 8 when stepDto is Etapa8Dto e8:
                AnalyzeEtapa8(e8, items);
                break;
        }

        return new ClarificationAnalysisResultDto(items.Count > 0, items);
    }

    private static void AnalyzeEtapa3(Etapa3Dto dto, List<ClarificationItemDto> items)
    {
        var answer35 = dto.PorQueEscolhemVoce;
        if (IsVagueOrShort(answer35, 20, GenericClichesEtapa3))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "3.5",
                FieldName: nameof(dto.PorQueEscolhemVoce),
                CurrentAnswer: answer35 ?? string.Empty,
                ReasonVague: "Sua resposta 'Por que os pacientes escolhem você' é genérica. Termos como 'humanizado', 'dedicado' ou 'preço justo' são esperados por qualquer paciente.",
                SuggestionTitle: "Aprofunde seu diferencial competitivo real",
                SuggestionPrompt: "O que acontece na prática durante a 1ª consulta que o paciente não encontra em nenhum outro lugar?",
                ExampleAnswer: "Exemplo: 'Na 1ª consulta apresento um escaneamento 3D detalhado com simulação do resultado final e ofereço acompanhamento via WhatsApp em 24h.'"
            ));
        }

        var answer34 = dto.DiferencialAtendimento;
        if (IsVagueOrShort(answer34, 20, GenericClichesEtapa3))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "3.4",
                FieldName: nameof(dto.DiferencialAtendimento),
                CurrentAnswer: answer34 ?? string.Empty,
                ReasonVague: "Sua resposta sobre diferencial de atendimento é genérica ou muito curta.",
                SuggestionTitle: "Especifique o diferencial de atendimento",
                SuggestionPrompt: "Como o seu atendimento se diferencia no dia a dia (ex: pontualidade rígida, café artesanal, consulta sem pressa de 1h30)?",
                ExampleAnswer: "Exemplo: 'Consulta sem pressa de 1h30 com escuta ativa e ambiente totalmente silencioso com aromaterapia.'"
            ));
        }
    }

    private static void AnalyzeEtapa4(Etapa4Dto dto, List<ClarificationItemDto> items)
    {
        var answerMedos = dto.MaioresMedos;
        if (IsVagueOrShort(answerMedos, 25, GenericClichesEtapa4))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "4.2",
                FieldName: nameof(dto.MaioresMedos),
                CurrentAnswer: answerMedos ?? string.Empty,
                ReasonVague: "A resposta é genérica. Para gerar roteiros de alto engajamento, precisamos da dor emocional exata do paciente.",
                SuggestionTitle: "Especifique a dor/medo emocional do seu cliente",
                SuggestionPrompt: "Qual é o pensamento exato que o seu cliente tem antes de dormir sobre o problema dele?",
                ExampleAnswer: "Exemplo: 'Tem vergonha de sorrir em fotos de família e evita eventos sociais por causa dos dentes tortos.'"
            ));
        }

        var answerDesejos = dto.MaioresDesejos;
        if (IsVagueOrShort(answerDesejos, 25, GenericClichesEtapa4))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "4.3",
                FieldName: nameof(dto.MaioresDesejos),
                CurrentAnswer: answerDesejos ?? string.Empty,
                ReasonVague: "A resposta sobre os maiores desejos do paciente é genérica ou muito vaga.",
                SuggestionTitle: "Especifique o desejo transformador do seu cliente",
                SuggestionPrompt: "Como o seu paciente quer se sentir após o tratamento?",
                ExampleAnswer: "Exemplo: 'Quer se olhar no espelho com orgulho e voltar a sorrir livremente sem cobrir a boca.'"
            ));
        }
    }

    private static void AnalyzeEtapa7(Etapa7Dto dto, List<ClarificationItemDto> items)
    {
        var answer = dto.VerdadeCorajosa;
        if (IsVagueOrShort(answer, 20, GenericClichesEtapa7))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "7.3",
                FieldName: nameof(dto.VerdadeCorajosa),
                CurrentAnswer: answer ?? string.Empty,
                ReasonVague: "Sua resposta sobre sua metodologia ou opinião forte é genérica ou muito curta.",
                SuggestionTitle: "Compartilhe uma verdade corajosa do seu mercado",
                SuggestionPrompt: "Qual mito da sua profissão você odeia ver outros profissionais espalhando?",
                ExampleAnswer: "Exemplo: 'Tratamento sem acompanhamento individualizado não funciona e só faz o paciente gastar dinheiro em vão.'"
            ));
        }
    }

    private static void AnalyzeEtapa8(Etapa8Dto dto, List<ClarificationItemDto> items)
    {
        var answer = dto.AmostraEscritaExplicativa;
        if (IsVagueOrShort(answer, 40, GenericClichesEtapa8))
        {
            items.Add(new ClarificationItemDto(
                QuestionId: "8.2",
                FieldName: nameof(dto.AmostraEscritaExplicativa),
                CurrentAnswer: answer ?? string.Empty,
                ReasonVague: "A amostra de texto é muito curta (< 40 caracteres) ou genérica.",
                SuggestionTitle: "Forneça uma amostra de escrita real suficiente",
                SuggestionPrompt: "Escreva de 2 a 3 frases exatamente como você falaria ou enviaria em um áudio/mensagem para um paciente.",
                ExampleAnswer: "Exemplo: 'Olha, o segredo não é fazer mil coisas ao mesmo tempo. É focar no básico com constância todos os dias. Se fizer isso, o resultado vem.'"
            ));
        }
    }

    public static bool IsVagueOrShort(string? text, int minLength, string[] clichês)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        var trimmed = text.Trim();
        if (trimmed.Length < minLength) return true;

        var lower = trimmed.ToLowerInvariant();
        return clichês.Any(c => lower.Contains(c));
    }
}
