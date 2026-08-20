using System.Text.Json;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public sealed class DynamicPromptEngine : IDynamicPromptEngine
{
    private readonly IPromptTemplateRepository _promptRepository;
    private readonly ICouncilRuleRepository? _councilRuleRepository;

    public DynamicPromptEngine(
        IPromptTemplateRepository promptRepository,
        ICouncilRuleRepository? councilRuleRepository = null)
    {
        _promptRepository = promptRepository;
        _councilRuleRepository = councilRuleRepository;
    }

    public async Task<LLMRequest> RenderPromptAsync(
        string agentName,
        IDictionary<string, string> variables,
        LLMRequest defaultFallbackRequest,
        CancellationToken cancellationToken = default)
    {
        var activeTemplate = await _promptRepository.GetActiveByAgentNameAsync(agentName, cancellationToken);
        if (activeTemplate == null)
        {
            return defaultFallbackRequest;
        }

        var systemPrompt = activeTemplate.SystemPrompt;
        var userPrompt = activeTemplate.UserPromptTemplate;

        // Se variáveis contiverem 'conselho' e não houver 'regras_conselho' preenchido, tenta buscar no repositório de regras
        if (_councilRuleRepository != null && !variables.ContainsKey("regras_conselho"))
        {
            string? councilAcronym = null;
            if (variables.TryGetValue("conselho", out var acronym) && !string.IsNullOrWhiteSpace(acronym))
            {
                councilAcronym = acronym;
            }
            else if (variables.TryGetValue("profissao", out var prof) && !string.IsNullOrWhiteSpace(prof))
            {
                councilAcronym = InferCouncilFromProfessao(prof);
            }

            if (!string.IsNullOrWhiteSpace(councilAcronym))
            {
                var rule = await _councilRuleRepository.GetByAcronymAsync(councilAcronym, cancellationToken);
                if (rule != null)
                {
                    variables["regras_conselho"] = $"[REGULAMENTAÇÃO {rule.CouncilAcronym} - {rule.ResolutionNumber}]: {rule.GuidelinesText}";
                }
            }
        }

        // Substituição de variáveis no User Prompt e System Prompt
        foreach (var (key, value) in variables)
        {
            var placeholder = "{{" + key + "}}";
            userPrompt = userPrompt.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
            systemPrompt = systemPrompt.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        // Parâmetros de execução LLM
        double temperature = defaultFallbackRequest.Temperature;
        int maxTokens = defaultFallbackRequest.MaxTokens;
        bool responseFormatJson = defaultFallbackRequest.ResponseFormatJson;

        try
        {
            if (!string.IsNullOrWhiteSpace(activeTemplate.ParametersJson))
            {
                using var jsonDoc = JsonDocument.Parse(activeTemplate.ParametersJson);
                var root = jsonDoc.RootElement;
                if (root.TryGetProperty("Temperature", out var tempProp) && tempProp.TryGetDouble(out var t))
                    temperature = t;
                if (root.TryGetProperty("MaxTokens", out var maxTokensProp) && maxTokensProp.TryGetInt32(out var m))
                    maxTokens = m;
                if (root.TryGetProperty("ResponseFormatJson", out var jsonProp))
                    responseFormatJson = jsonProp.GetBoolean();
            }
        }
        catch
        {
            // Mantém os parâmetros default de fallback em caso de falha no parse do JSON
        }

        return new LLMRequest
        {
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            Temperature = temperature,
            MaxTokens = maxTokens,
            ResponseFormatJson = responseFormatJson
        };
    }

    private static string? InferCouncilFromProfessao(string profissao)
    {
        var p = profissao.ToLowerInvariant();
        if (p.Contains("médic") || p.Contains("medic") || p.Contains("dermatolog") || p.Contains("cirurgiã") || p.Contains("pediatra"))
            return "CFM";
        if (p.Contains("dentis") || p.Contains("odont"))
            return "CRO";
        if (p.Contains("biomédic") || p.Contains("biomedic"))
            return "CRBM";

        return null;
    }
}
