using Microsoft.EntityFrameworkCore;
using PersonaScript.Modules.Backoffice.Domain;

namespace PersonaScript.Modules.Backoffice.Infrastructure.Persistence;

public static class PromptTemplateSeeder
{
    public static async Task SeedDefaultPromptsAsync(BackofficeDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.PromptTemplates.AnyAsync(cancellationToken))
        {
            return;
        }

        var defaultPrompts = new[]
        {
            PromptTemplate.Create(
                agentName: "Agent1_Diagnosis",
                version: 1,
                systemPrompt: @"Você é o Agente 1 (Estrategista de Persona e Posicionamento de Marca) do sistema PersonaScript AI.
Sua missão é analisar o perfil completo de um profissional (Anamnese em 10 Etapas) e construir o Diagnóstico de Posicionamento de Marca Estratégico.

Você DEVE responder EXCLUSIVAMENTE em formato JSON estrito.",
                userPromptTemplate: @"# FICHA DE ANAMNESE COMPLETA DO PROFISSIONAL

{{AnamneseData}}

Gere o Diagnóstico de Posicionamento completo em JSON conforme as instruções.",
                parametersJson: "{\"Temperature\": 0.5, \"MaxTokens\": 2500, \"ResponseFormatJson\": true}",
                description: "Versão padrão do Agente 1 - Diagnóstico de Posicionamento",
                adminEmail: "sistema@personascript.ai",
                isActive: true).Value,

            PromptTemplate.Create(
                agentName: "Agent2_VideoScript",
                version: 1,
                systemPrompt: @"Você é o Agente 2 - Copywriter de Vídeo de Alta Conversão do PersonaScript AI.
Sua missão é criar um roteiro de vídeo magnético, ético e focado em engajamento para redes sociais (Reels, TikTok, Shorts).",
                userPromptTemplate: @"--- PERFIL DO PROFISSIONAL ---
Nome: {{Nome}}
Profissão: {{Profissao}}
Frase de Posicionamento: {{FrasePosicionamento}}

--- ESPECIFICAÇÕES DO ROTEIRO ---
Tema: {{Tema}}
Pilar: {{PilarConteudo}}
Objetivo: {{Objetivo}}

Gere o roteiro estruturado nos blocos: Gancho, Retenção, CTA, Legenda e Dicas de Gravação.",
                parametersJson: "{\"Temperature\": 0.7, \"MaxTokens\": 2000, \"ResponseFormatJson\": true}",
                description: "Versão padrão do Agente 2 - Roteiro de Vídeo",
                adminEmail: "sistema@personascript.ai",
                isActive: true).Value,

            PromptTemplate.Create(
                agentName: "Agent2_Stories",
                version: 1,
                systemPrompt: @"Você é o Agente 2 - Especialista em Sequência de Stories de Alta Retenção do PersonaScript AI.
Sua missão é criar uma sequência estratégica de 3 a 5 Stories para redes sociais.",
                userPromptTemplate: @"--- SOLICITAÇÃO DE STORIES ---
Tema Central: {{Tema}}
Objetivo: {{Objetivo}}
Tom de Voz: {{TomDeVoz}}

Gere a sequência em JSON contendo: Tela, TextoFalado, ElementoInterativo, DicaVisual.",
                parametersJson: "{\"Temperature\": 0.7, \"MaxTokens\": 1500, \"ResponseFormatJson\": true}",
                description: "Versão padrão do Agente 2 - Sequência de Stories",
                adminEmail: "sistema@personascript.ai",
                isActive: true).Value,

            PromptTemplate.Create(
                agentName: "Agent2_AnamneseClarification",
                version: 1,
                systemPrompt: @"Você é o Agente de Clarificação de Anamnese do PersonaScript AI.
Sua missão é analisar respostas ambíguas ou incompletas fornecidas pelo profissional e gerar perguntas clarificadoras gentis e focadas.",
                userPromptTemplate: @"--- DADOS DA ETAPA ---
Etapa: {{EtapaNome}}
Resposta Atual: {{RespostaAtual}}

Gere de 1 a 3 perguntas de aprofundamento claras e amigáveis.",
                parametersJson: "{\"Temperature\": 0.4, \"MaxTokens\": 800, \"ResponseFormatJson\": true}",
                description: "Versão padrão do Agente 2 - Clarificação de Anamnese",
                adminEmail: "sistema@personascript.ai",
                isActive: true).Value
        };

        await dbContext.PromptTemplates.AddRangeAsync(defaultPrompts, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
