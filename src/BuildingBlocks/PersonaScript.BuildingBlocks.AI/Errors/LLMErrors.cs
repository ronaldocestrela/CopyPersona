using PersonaScript.BuildingBlocks.Results;

namespace PersonaScript.BuildingBlocks.AI.Errors;

public static class LLMErrors
{
    public static Error ProviderUnavailable(string providerName) =>
        new("AI.ProviderUnavailable", $"O provedor de IA '{providerName}' está indisponível ou falhou.");

    public static Error RateLimitExceeded(string providerName) =>
        new("AI.RateLimitExceeded", $"Limite de taxa (Rate limit HTTP 429) excedido para o provedor '{providerName}'.");

    public static Error Timeout(string providerName) =>
        new("AI.Timeout", $"Tempo limite esgotado durante a chamada ao provedor '{providerName}'.");

    public static Error InvalidJsonResponse(string details) =>
        new("AI.InvalidJsonResponse", $"A resposta da LLM não é um JSON válido no formato esperado: {details}");

    public static Error AllProvidersFailed(string details) =>
        new("AI.AllProvidersFailed", $"Todos os provedores de LLM configurados falharam. Detalhes: {details}");

    public static Error MissingApiKey(string providerName) =>
        new("AI.MissingApiKey", $"A chave de API para o provedor '{providerName}' não foi configurada.");
}
