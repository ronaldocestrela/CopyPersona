using System.Text;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Modules.Anamnese.Application.DTOs;

namespace PersonaScript.Modules.Personas.Application.Services;

public sealed class PersonaPromptBuilder : IPersonaPromptBuilder
{
    public LLMRequest BuildPrompt(FullAnamneseDto anamnese, string? feedback = null)
    {
        var systemPrompt = @"Você é o Agente 1 (Estrategista de Persona e Posicionamento de Marca) do sistema PersonaScript AI.
Sua missão é analisar o perfil completo de um profissional (Anamnese em 10 Etapas) e construir o Diagnóstico de Posicionamento de Marca Estratégico.

Você DEVE responder EXCLUSIVAMENTE em formato JSON estrito aderente à estrutura definida abaixo:

{
  ""frasePosicionamento"": ""Frase única de posicionamento de mercado clara, impactante e memorável"",
  ""sintesePerfil"": ""Resumo do perfil profissional e da dor central do paciente/cliente ideal"",
  ""tomDeVoz"": ""Descrição do tom de voz e estilo de comunicação"",
  ""estiloVisualSugerido"": ""Recomendação de identidade e estética visual"",
  ""arquetipoPrincipal"": ""Arquétipo de marca principal (ex: O Sábio, O Especialista, O Herói)"",
  ""arquetipoSecundario"": ""Arquétipo de marca secundário"",
  ""pilaresConteudo"": [
    {
      ""nome"": ""Nome do Pilar"",
      ""percentual"": 30,
      ""descricao"": ""Explicação do objetivo deste pilar"",
      ""exemplosTopicos"": [""Tópico 1"", ""Tópico 2""]
    }
  ],
  ""temasProibidos"": [""Tema proibido 1""],
  ""palavrasEvitar"": [""Palavra a evitar 1""],
  ""diretrizesInegociaveis"": [""Diretriz inegociável 1""],
  ""limitesExposicao"": ""Descrição dos limites de exposição pessoal e profissional""
}

REGRAS OBRIGATÓRIAS:
1. O somatório do campo 'percentual' de todos os itens em 'pilaresConteudo' DEVE ser EXATAMENTE igual a 100.
2. Crie entre 3 e 5 pilares de conteúdo equilibrados (ex: Educação, Prova/Casos, Autoridade, Conexão/Bastidores).
3. Incorpore com rigor máximo as proibições, termos a evitar e limites de exposição indicados nas Etapas 5, 6 e 8.";

        var userPrompt = BuildUserPrompt(anamnese, feedback);

        return new LLMRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = 0.5,
            MaxTokens = 2500,
            ResponseFormatJson = true
        };
    }

    private static string BuildUserPrompt(FullAnamneseDto anamnese, string? feedback = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# FICHA DE ANAMNESE COMPLETA DO PROFISSIONAL");
        sb.AppendLine();

        if (anamnese.Etapa1 is not null)
        {
            sb.AppendLine("## ETAPA 1: QUEM É VOCÊ");
            sb.AppendLine($"- Nome Completo: {anamnese.Etapa1.NomeCompleto}");
            sb.AppendLine($"- Como Gosta de Ser Chamado: {anamnese.Etapa1.ComoGostaSerChamado}");
            sb.AppendLine($"- Profissão e Especialidade: {anamnese.Etapa1.ProfissaoEspecialidade}");
            sb.AppendLine($"- Anos de Atuação: {anamnese.Etapa1.TempoAtuacaoAnos}");
            sb.AppendLine($"- Formações/Especializações: {anamnese.Etapa1.FormacoesEspecializacoes}");
            sb.AppendLine($"- Prêmios e Títulos: {anamnese.Etapa1.PremiosTitulos}");
            sb.AppendLine();
        }

        if (anamnese.Etapa2 is not null)
        {
            sb.AppendLine("## ETAPA 2: SUA HISTÓRIA");
            sb.AppendLine($"- Motivação da Escolha: {anamnese.Etapa2.MotivacaoEscolha}");
            sb.AppendLine($"- Caso Marcante: {anamnese.Etapa2.CasoMarcante}");
            sb.AppendLine($"- Fase Mais Difícil: {anamnese.Etapa2.FaseMaisDificil}");
            sb.AppendLine($"- Motor Pessoal: {anamnese.Etapa2.MotorPessoal}");
            sb.AppendLine();
        }

        if (anamnese.Etapa3 is not null)
        {
            sb.AppendLine("## ETAPA 3: SEU TRABALHO");
            sb.AppendLine($"- Procedimento Master: {anamnese.Etapa3.ProcedimentoMaster}");
            sb.AppendLine($"- Procedimento Lucrativo: {anamnese.Etapa3.ProcedimentoLucrativo}");
            sb.AppendLine($"- Procedimento Preferido: {anamnese.Etapa3.ProcedimentoPreferido}");
            sb.AppendLine($"- Diferencial de Atendimento: {anamnese.Etapa3.DiferencialAtendimento}");
            sb.AppendLine($"- Por que Escolhem Você: {anamnese.Etapa3.PorQueEscolhemVoce}");
            sb.AppendLine($"- Crítica aos Pares: {anamnese.Etapa3.CriticaAosPares}");
            sb.AppendLine();
        }

        if (anamnese.Etapa4 is not null)
        {
            sb.AppendLine("## ETAPA 4: SEU PACIENTE / CLIENTE IDEAL");
            sb.AppendLine($"- Perfil Demográfico/Psicográfico: {anamnese.Etapa4.PerfilDemograficoPsicografico}");
            sb.AppendLine($"- Maiores Medos: {anamnese.Etapa4.MaioresMedos}");
            sb.AppendLine($"- Maiores Desejos: {anamnese.Etapa4.MaioresDesejos}");
            sb.AppendLine($"- Perguntas Frequentes: {anamnese.Etapa4.PerguntasFrequentes}");
            sb.AppendLine($"- Mitos/Informações Erradas: {anamnese.Etapa4.MitosInformacoesErradas}");
            sb.AppendLine();
        }

        if (anamnese.Etapa5 is not null)
        {
            sb.AppendLine("## ETAPA 5: SUAS REFERÊNCIAS");
            sb.AppendLine($"- Perfis na Área: {string.Join(", ", anamnese.Etapa5.PerfisArea)}");
            sb.AppendLine($"- O que Admira na Área: {anamnese.Etapa5.OQueAdmiraArea}");
            sb.AppendLine($"- O que NÃO Faria na Área (Proibições 5.3): {anamnese.Etapa5.OQueNaoFariaArea}");
            sb.AppendLine($"- Perfis Fora da Área: {string.Join(", ", anamnese.Etapa5.PerfisForaArea)}");
            sb.AppendLine($"- O que Atrai Fora da Área: {anamnese.Etapa5.OQueAtraiForaArea}");
            sb.AppendLine();
        }

        if (anamnese.Etapa6 is not null)
        {
            sb.AppendLine("## ETAPA 6: LIMITES DE EXPOSIÇÃO (Etapa 6.1)");
            sb.AppendLine($"- Assuntos Proibidos: {anamnese.Etapa6.AssuntosProibidos}");
            sb.AppendLine($"- Vida Pessoal Aceita: {anamnese.Etapa6.VidaPessoalAceita}");
            sb.AppendLine($"- Estilo de Vida Aceito: {anamnese.Etapa6.EstiloVidaAceito}");
            sb.AppendLine($"- Trabalho Aceito: {anamnese.Etapa6.TrabalhoAceito}");
            sb.AppendLine($"- Regras do Conselho Regional: {anamnese.Etapa6.RegrasConselhoRegional}");
            sb.AppendLine();
        }

        if (anamnese.Etapa7 is not null)
        {
            sb.AppendLine("## ETAPA 7: SEU CONHECIMENTO");
            sb.AppendLine($"- Temas Favoritos: {anamnese.Etapa7.TemasFavoritos}");
            sb.AppendLine($"- Tema da Palestra: {anamnese.Etapa7.TemaPalestra}");
            sb.AppendLine($"- Verdade Corajosa: {anamnese.Etapa7.VerdadeCorajosa}");
            sb.AppendLine();
        }

        if (anamnese.Etapa8 is not null)
        {
            sb.AppendLine("## ETAPA 8: SEU JEITO (Etapa 8.4)");
            sb.AppendLine($"- Arquétipos de Comunicação: {string.Join(", ", anamnese.Etapa8.ArquetiposComunicacao)}");
            sb.AppendLine($"- Amostra Escrita Explicativa: {anamnese.Etapa8.AmostraEscritaExplicativa}");
            sb.AppendLine($"- Estética Odiada: {anamnese.Etapa8.EsteticaOdiada}");
            sb.AppendLine();
        }

        if (anamnese.Etapa9 is not null)
        {
            sb.AppendLine("## ETAPA 9: ROTINA E CAPACIDADE");
            sb.AppendLine($"- Dia Típico/Rotina: {anamnese.Etapa9.DiaTipicoRotina}");
            sb.AppendLine($"- Horas/Semana para Conteúdo: {anamnese.Etapa9.HorasSemanaConteudo}");
            sb.AppendLine();
        }

        if (anamnese.Etapa10 is not null)
        {
            sb.AppendLine("## ETAPA 10: OBJETIVOS DE NEGÓCIO");
            sb.AppendLine($"- Meta em 3 Meses: {anamnese.Etapa10.Meta3Meses}");
            sb.AppendLine($"- Meta em 1 Ano: {anamnese.Etapa10.Meta1Ano}");
            sb.AppendLine();
        }

        if (!string.IsNullOrWhiteSpace(feedback))
        {
            sb.AppendLine("## INSTRUÇÕES E FEEDBACK DE REFINAMENTO DO USUÁRIO:");
            sb.AppendLine(feedback);
            sb.AppendLine();
        }

        sb.AppendLine("Gere o Diagnóstico de Posicionamento completo em JSON conforme as instruções.");
        return sb.ToString();
    }
}
