using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PersonaScript.Server.Observability.Alerts;

public sealed class SlackTeamsWebhookAlertService : IOperationalAlertService
{
    private readonly HttpClient _httpClient;
    private readonly AlertOptions _options;
    private readonly ILogger<SlackTeamsWebhookAlertService> _logger;

    public SlackTeamsWebhookAlertService(
        HttpClient httpClient,
        IOptions<AlertOptions> options,
        ILogger<SlackTeamsWebhookAlertService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new AlertOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> NotifyPaymentWebhookFailureAsync(PaymentWebhookFailureAlert alert, CancellationToken cancellationToken = default)
    {
        var title = $"🚨 [Stripe Webhook Failure] {_options.EnvironmentName}";
        var summary = $"Falha no processamento do webhook `{alert.EventType}`.";
        var fields = new Dictionary<string, string>
        {
            ["Ambiente"] = _options.EnvironmentName,
            ["Evento"] = alert.EventType,
            ["Cliente ID"] = alert.CustomerId ?? "N/A",
            ["Código Erro"] = alert.ErrorCode ?? "N/A",
            ["Mensagem"] = alert.ErrorMessage,
            ["Timestamp"] = alert.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        return await SendAlertAsync(title, summary, fields, isCritical: true, cancellationToken);
    }

    public async Task<bool> NotifyLLMQuotaOrFailureAsync(LLMFailureAlert alert, CancellationToken cancellationToken = default)
    {
        var severity = alert.AllProvidersFailed ? "🚨 CRÍTICO" : "⚠️ AVISO";
        var title = $"{severity} [LLM Fallback / Quota Exceeded] {_options.EnvironmentName}";
        var summary = alert.AllProvidersFailed
            ? "Todos os provedores de LLM falharam! Nenhuma resposta pôde ser gerada."
            : $"Provedor `{alert.ProviderType}` atingiu quota ou retornou erro.";

        var fields = new Dictionary<string, string>
        {
            ["Ambiente"] = _options.EnvironmentName,
            ["Provedor"] = alert.ProviderType,
            ["Modelo"] = alert.Model,
            ["Código Erro"] = alert.ErrorCode ?? "N/A",
            ["Mensagem"] = alert.ErrorMessage,
            ["Latência"] = $"{alert.LatencyMs}ms",
            ["Falha Geral"] = alert.AllProvidersFailed ? "SIM" : "NÃO",
            ["Timestamp"] = alert.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        return await SendAlertAsync(title, summary, fields, isCritical: alert.AllProvidersFailed, cancellationToken);
    }

    public async Task<bool> NotifySystemCriticalErrorAsync(SystemCriticalAlert alert, CancellationToken cancellationToken = default)
    {
        var title = $"🔥 [System Error] {_options.EnvironmentName}: {alert.Title}";
        var summary = $"Erro no componente `{alert.Component}`.";
        var fields = new Dictionary<string, string>
        {
            ["Ambiente"] = _options.EnvironmentName,
            ["Componente"] = alert.Component,
            ["Mensagem"] = alert.Message,
            ["Stack"] = string.IsNullOrWhiteSpace(alert.StackTrace) ? "N/A" : alert.StackTrace.Substring(0, Math.Min(300, alert.StackTrace.Length)),
            ["Timestamp"] = alert.TimestampUtc.ToString("yyyy-MM-dd HH:mm:ss UTC")
        };

        return await SendAlertAsync(title, summary, fields, isCritical: true, cancellationToken);
    }

    private async Task<bool> SendAlertAsync(
        string title,
        string summary,
        IReadOnlyDictionary<string, string> fields,
        bool isCritical,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("[AlertService] Alertas desabilitados. Ignorando envio para: {Title}", title);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.WebhookUrl))
        {
            _logger.LogWarning("[AlertService] WebhookUrl não configurada. Alerta registrado em log: {Title} - {Summary}", title, summary);
            return false;
        }

        try
        {
            object payload = _options.ChannelType == AlertChannelType.Slack
                ? BuildSlackPayload(title, summary, fields, isCritical)
                : BuildTeamsPayload(title, summary, fields, isCritical);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_options.WebhookUrl, jsonContent, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[AlertService] Alerta enviado com sucesso ({ChannelType}): {Title}", _options.ChannelType, title);
                return true;
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("[AlertService] Falha ao enviar alerta ({StatusCode}): {ErrorBody}", response.StatusCode, errorBody);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AlertService] Exceção ao despachar alerta para {WebhookUrl}", _options.WebhookUrl);
            return false;
        }
    }

    private static object BuildSlackPayload(
        string title,
        string summary,
        IReadOnlyDictionary<string, string> fields,
        bool isCritical)
    {
        var color = isCritical ? "#E01E5A" : "#ECB22E";
        var fieldsList = fields.Select(f => new
        {
            title = f.Key,
            value = f.Value,
            @short = true
        }).ToList();

        return new
        {
            text = $"{title}\n{summary}",
            attachments = new[]
            {
                new
                {
                    color,
                    title,
                    text = summary,
                    fields = fieldsList,
                    footer = "PersonaScript AI Observability",
                    ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                }
            }
        };
    }

    private static object BuildTeamsPayload(
        string title,
        string summary,
        IReadOnlyDictionary<string, string> fields,
        bool isCritical)
    {
        var themeColor = isCritical ? "E01E5A" : "ECB22E";
        var facts = fields.Select(f => new
        {
            name = f.Key,
            value = f.Value
        }).ToList();

        return new
        {
            type = "MessageCard",
            context = "http://schema.org/extensions",
            themeColor,
            summary = title,
            sections = new[]
            {
                new
                {
                    activityTitle = title,
                    activitySubtitle = summary,
                    facts
                }
            }
        };
    }
}
