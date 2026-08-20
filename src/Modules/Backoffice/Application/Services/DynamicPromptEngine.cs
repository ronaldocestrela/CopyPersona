using System.Text.Json;
using PersonaScript.BuildingBlocks.AI.Models;
using PersonaScript.Modules.Backoffice.Domain.Repositories;

namespace PersonaScript.Modules.Backoffice.Application.Services;

public sealed class DynamicPromptEngine : IDynamicPromptEngine
{
    private readonly IPromptTemplateRepository _promptRepository;

    public DynamicPromptEngine(IPromptTemplateRepository promptRepository)
    {
        _promptRepository = promptRepository;
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
            // Se nenhum prompt estiver cadastrado no banco, utiliza o fallback nativo de código
            return defaultFallbackRequest;
        }

        var systemPrompt = activeTemplate.SystemPrompt;
        var userPrompt = activeTemplate.UserPromptTemplate;

        // Substituição de variáveis no User Prompt
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
}
