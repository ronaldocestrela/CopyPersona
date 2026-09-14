using System.Diagnostics.Metrics;

namespace PersonaScript.Server.Observability.Metrics;

public static class PersonaScriptMetrics
{
    public const string MeterName = "PersonaScript.Observability";
    private static readonly Meter s_meter = new(MeterName, "1.0.0");

    private static readonly Counter<long> s_llmRequestsCounter = s_meter.CreateCounter<long>(
        "personscript.llm.requests",
        unit: "{request}",
        description: "Contador de requisições enviadas aos provedores de LLM.");

    private static readonly Counter<long> s_llmFailuresCounter = s_meter.CreateCounter<long>(
        "personscript.llm.failures",
        unit: "{failure}",
        description: "Contador de falhas e rate limits em provedores de LLM.");

    private static readonly Counter<long> s_stripeWebhookFailuresCounter = s_meter.CreateCounter<long>(
        "personscript.billing.webhook_failures",
        unit: "{failure}",
        description: "Contador de falhas na validação ou processamento de webhooks do Stripe.");

    private static readonly Histogram<double> s_llmDurationHistogram = s_meter.CreateHistogram<double>(
        "personscript.llm.duration.ms",
        unit: "ms",
        description: "Histograma de latência das chamadas para LLM em milissegundos.");

    public static void RecordLLMRequest(string providerType, string model)
    {
        s_llmRequestsCounter.Add(1,
            new KeyValuePair<string, object?>("provider", providerType),
            new KeyValuePair<string, object?>("model", model));
    }

    public static void RecordLLMFailure(string providerType, string errorCode)
    {
        s_llmFailuresCounter.Add(1,
            new KeyValuePair<string, object?>("provider", providerType),
            new KeyValuePair<string, object?>("error_code", errorCode));
    }

    public static void RecordStripeWebhookFailure(string eventType, string errorCode)
    {
        s_stripeWebhookFailuresCounter.Add(1,
            new KeyValuePair<string, object?>("event_type", eventType),
            new KeyValuePair<string, object?>("error_code", errorCode));
    }

    public static void RecordLLMDuration(string providerType, double durationMs)
    {
        s_llmDurationHistogram.Record(durationMs,
            new KeyValuePair<string, object?>("provider", providerType));
    }
}
