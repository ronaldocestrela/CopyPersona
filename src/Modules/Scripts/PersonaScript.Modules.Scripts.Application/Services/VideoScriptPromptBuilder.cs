using System.Text;
using PersonaScript.Modules.Anamnese.Application.DTOs;
using PersonaScript.Modules.Personas.Domain;

namespace PersonaScript.Modules.Scripts.Application.Services;

public sealed class VideoScriptPromptBuilder : IVideoScriptPromptBuilder
{
    public string BuildPrompt(
        FullAnamneseDto anamnese,
        PersonaDiagnosis? diagnosis,
        string tema,
        string pilarConteudo,
        string objetivo,
        string? tomDesejado = null,
        string? instrucoesAdicionais = null)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Você é o Agente 2 - Copywriter de Vídeo de Alta Conversão do PersonaScript AI.");
        sb.AppendLine("Sua missão é criar um roteiro de vídeo magnético, ético e focado em engajamento para redes sociais (Reels, TikTok, Shorts).");
        sb.AppendLine();

        // Dados do Profissional
        var nome = anamnese.Etapa1?.ComoGostaSerChamado ?? anamnese.Etapa1?.NomeCompleto ?? "Profissional";
        var profissao = anamnese.Etapa1?.ProfissaoEspecialidade ?? "Especialista";
        sb.AppendLine($"--- PERFIL DO PROFISSIONAL ---");
        sb.AppendLine($"Nome: {nome}");
        sb.AppendLine($"Profissão/Especialidade: {profissao}");

        if (diagnosis != null)
        {
            sb.AppendLine($"Frase de Posicionamento: {diagnosis.FrasePosicionamento}");
            sb.AppendLine($"Tom de Voz Base: {diagnosis.IdentidadeMarca.TomDeVoz}");
        }
        sb.AppendLine();

        // 1. Amostra de Escrita Real (Etapa 8.2) para Clonagem de Tom de Voz
        var escritaReal = anamnese.Etapa8?.AmostraEscritaExplicativa;
        if (!string.IsNullOrWhiteSpace(escritaReal))
        {
            sb.AppendLine("--- CLONAGEM DE TOM DE VOZ (Etapa 8.2 - Amostra de Escrita Real) ---");
            sb.AppendLine("Exemplo de escrita real do profissional:");
            sb.AppendLine($"\"{escritaReal}\"");
            sb.AppendLine("INSTRUÇÃO: Estude o estilo de escrita acima para clonar a cadência e o tom de voz humano característico deste profissional.");
            sb.AppendLine();
        }

        // 2. Restrições Inegociáveis (5.3, 6.1, 8.4 e 6.6)
        sb.AppendLine("--- MATRIZ DE RESTRIÇÕES E DIRETRIZES INEGOCIÁVEIS ---");
        if (diagnosis?.MatrizRestricoes != null)
        {
            if (diagnosis.MatrizRestricoes.TemasProibidos.Count > 0)
                sb.AppendLine($"- Temas Proibidos: {string.Join(", ", diagnosis.MatrizRestricoes.TemasProibidos)}");
            if (diagnosis.MatrizRestricoes.PalavrasEvitar.Count > 0)
                sb.AppendLine($"- Palavras a Evitar: {string.Join(", ", diagnosis.MatrizRestricoes.PalavrasEvitar)}");
            if (diagnosis.MatrizRestricoes.DiretrizesInegociaveis.Count > 0)
                sb.AppendLine($"- Diretrizes Inegociáveis: {string.Join(", ", diagnosis.MatrizRestricoes.DiretrizesInegociaveis)}");
        }

        if (!string.IsNullOrWhiteSpace(anamnese.Etapa5?.OQueNaoFariaArea))
            sb.AppendLine($"- O que não faria na área (5.3): {anamnese.Etapa5.OQueNaoFariaArea}");

        if (!string.IsNullOrWhiteSpace(anamnese.Etapa6?.AssuntosProibidos))
            sb.AppendLine($"- Assuntos Proibidos (6.1): {anamnese.Etapa6.AssuntosProibidos}");

        if (!string.IsNullOrWhiteSpace(anamnese.Etapa6?.RegrasConselhoRegional))
            sb.AppendLine($"- Regras do Conselho Regional / Ética (6.6): {anamnese.Etapa6.RegrasConselhoRegional}");
        sb.AppendLine();

        // 3. Parâmetros da Solicitação do Roteiro
        sb.AppendLine("--- ESPECIFICAÇÕES DO ROTEIRO SOLICITADO ---");
        sb.AppendLine($"Tema do Vídeo: {tema}");
        sb.AppendLine($"Pilar de Conteúdo: {pilarConteudo}");
        sb.AppendLine($"Objetivo: {objetivo}");

        if (!string.IsNullOrWhiteSpace(tomDesejado))
            sb.AppendLine($"Tom Específico Desejado: {tomDesejado}");

        if (!string.IsNullOrWhiteSpace(instrucoesAdicionais))
            sb.AppendLine($"Instruções Adicionais: {instrucoesAdicionais}");
        sb.AppendLine();

        // 4. Estrutura Obrigatória dos 3 Blocos
        sb.AppendLine("--- ESTRUTURA OBRIGATÓRIA DA RESPOSTA (JSON) ---");
        sb.AppendLine("Você DEVE retornar o roteiro estruturado nos seguintes blocos:");
        sb.AppendLine("1. Gancho (Hook): Primeiros 3 segundos para parar a rolagem do feed.");
        sb.AppendLine("2. Retenção (Body): Conteúdo prático, direto e com embasamento mantendo a atenção.");
        sb.AppendLine("3. Chamada para Ação (CTA): Direcionamento claro, ético e persuasivo (comentário, direct ou agendamento).");
        sb.AppendLine("4. Legenda Sugerida: Legenda complementar pronta para publicação.");
        sb.AppendLine("5. Dicas de Gravação: Orientações de postura, enquadramento e entonação.");
        sb.AppendLine("6. Tom de Voz Aplicado: Breve descrição do tom alcançado.");

        return sb.ToString();
    }
}
