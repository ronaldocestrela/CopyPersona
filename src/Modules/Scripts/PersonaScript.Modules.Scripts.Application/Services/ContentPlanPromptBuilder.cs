using System.Text;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Scripts.Application.Services;

public sealed class ContentPlanPromptBuilder : IContentPlanPromptBuilder
{
    public string BuildPrompt(FullAnamneseDto anamnese, PersonaDiagnosis? diagnosis)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Você é o Agente 2 - Estrategista e Copywriter de Conteúdo do PersonaScript AI.");
        sb.AppendLine("Sua missão é gerar 2 entregáveis estratégicos essenciais para o profissional de saúde:");
        sb.AppendLine("1. PLANO DE STORIES DIÁRIOS: Um cronograma personalizado alinhado com a rotina real do profissional.");
        sb.AppendLine("2. CALENDÁRIO EDITORIAL DE 90 DIAS: Um planejamento trimestral de 12 semanas adaptado à facilidade de produção do profissional.");
        sb.AppendLine();

        // 1. Dados e Perfil do Profissional
        var nome = anamnese.Etapa1?.ComoGostaSerChamado ?? anamnese.Etapa1?.NomeCompleto ?? "Profissional";
        var profissao = anamnese.Etapa1?.ProfissaoEspecialidade ?? "Especialista";
        sb.AppendLine("--- PERFIL DO PROFISSIONAL ---");
        sb.AppendLine($"Nome: {nome}");
        sb.AppendLine($"Profissão/Especialidade: {profissao}");

        if (diagnosis != null)
        {
            sb.AppendLine($"Frase de Posicionamento: {diagnosis.FrasePosicionamento}");
            sb.AppendLine($"Tom de Voz: {diagnosis.IdentidadeMarca.TomDeVoz}");
            if (diagnosis.PilaresConteudo.Count > 0)
            {
                sb.AppendLine("Pilares de Conteúdo:");
                foreach (var pilar in diagnosis.PilaresConteudo)
                {
                    sb.AppendLine($"  - {pilar.Nome} ({pilar.Percentual}%): {pilar.Descricao}");
                }
            }
        }
        sb.AppendLine();

        // 2. Rotina Real e Capacidade (Etapa 9)
        sb.AppendLine("--- DADOS DE ROTINA E CAPACIDADE (ETAPA 9 DA ANAMNESE) ---");
        var diaTipico = anamnese.Etapa9?.DiaTipicoRotina;
        if (!string.IsNullOrWhiteSpace(diaTipico))
            sb.AppendLine($"Dia Típico (9.1): {diaTipico}");

        var tempoSemana = anamnese.Etapa9?.HorasSemanaConteudo;
        if (!string.IsNullOrWhiteSpace(tempoSemana))
            sb.AppendLine($"Tempo Disponível Semanal (9.2): {tempoSemana}");

        var facilidades = anamnese.Etapa9?.RankingFacilidadeFormatos;
        if (!string.IsNullOrWhiteSpace(facilidades))
            sb.AppendLine($"Ranking de Facilidade de Formatos (9.4): {facilidades}");

        sb.AppendLine();

        // 3. Tom de Voz Real (Etapa 8.2)
        var escritaReal = anamnese.Etapa8?.AmostraEscritaExplicativa;
        if (!string.IsNullOrWhiteSpace(escritaReal))
        {
            sb.AppendLine("--- AMOSTRA DE ESCRITA REAL (ETAPA 8.2) ---");
            sb.AppendLine($"\"{escritaReal}\"");
            sb.AppendLine();
        }

        // 4. Objetivos (Etapa 10)
        sb.AppendLine("--- OBJETIVOS (ETAPA 10) ---");
        var obj3Meses = anamnese.Etapa10?.Meta3Meses;
        if (!string.IsNullOrWhiteSpace(obj3Meses))
            sb.AppendLine($"Meta 3 Meses (10.1): {obj3Meses}");

        var resultadoPrioritario = anamnese.Etapa10?.ResultadoPrioritario.ToString();
        if (!string.IsNullOrWhiteSpace(resultadoPrioritario))
            sb.AppendLine($"Resultado #1 Prioritário (10.4): {resultadoPrioritario}");

        sb.AppendLine();

        // 5. Restrições
        sb.AppendLine("--- RESTRIÇÕES E DIRETRIZES ÉTICAS ---");
        if (diagnosis?.MatrizRestricoes != null)
        {
            if (diagnosis.MatrizRestricoes.TemasProibidos.Count > 0)
                sb.AppendLine($"- Temas Proibidos: {string.Join(", ", diagnosis.MatrizRestricoes.TemasProibidos)}");
            if (diagnosis.MatrizRestricoes.PalavrasEvitar.Count > 0)
                sb.AppendLine($"- Palavras a Evitar: {string.Join(", ", diagnosis.MatrizRestricoes.PalavrasEvitar)}");
        }
        if (!string.IsNullOrWhiteSpace(anamnese.Etapa6?.AssuntosProibidos))
            sb.AppendLine($"- Assuntos Proibidos (6.1): {anamnese.Etapa6.AssuntosProibidos}");
        if (!string.IsNullOrWhiteSpace(anamnese.Etapa6?.RegrasConselhoRegional))
            sb.AppendLine($"- Regras do Conselho Regional (6.6): {anamnese.Etapa6.RegrasConselhoRegional}");

        sb.AppendLine();

        // 6. Diretrizes de Resposta JSON
        sb.AppendLine("--- DIRETRIZES DA RESPOSTA JSON ---");
        sb.AppendLine("Retorne a resposta estritamente no schema JSON solicitado:");
        sb.AppendLine("1. plano_stories: contendo frequencia_diaria_recomendada, blocos_horarios (com periodo, horario_sugestao, gatilho_rotina, tipo_conteudo, exemplo_pratico, objetivo_conexao) e diretrizes_humanizacao.");
        sb.AppendLine("2. calendario_90_dias: contendo objetivo_trimestral e exatamente 12 semanas (semanas de 1 a 12), cada uma com numero_semana, tema_central, pilar_conteudo, objetivo_estrategico, sugestao_formato e ideias_conteudo (lista de 2 a 3 sugestões).");

        return sb.ToString();
    }
}
